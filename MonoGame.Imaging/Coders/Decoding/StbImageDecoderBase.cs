using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using MonoGame.Framework;
using MonoGame.Framework.Memory;
using MonoGame.Framework.Vectors;
using MonoGame.Imaging.Attributes.Coder;
using MonoGame.Imaging.Coders.Detection;
using MonoGame.Imaging.Utilities;
using StbSharp.ImageRead;

namespace MonoGame.Imaging.Coders.Decoding
{
    public abstract partial class StbImageDecoderBase<TOptions> : IImageDecoder, IProgressReportingCoder<IImageDecoder>
        where TOptions : DecoderOptions
    {
        private readonly object _progressMutex = new();
        private ImagingProgressCallback<IImageDecoder>? _progress;
        private ReadProgressCallback? _readProgress;

        private Image.ConvertPixelDataDelegate? _convertPixels;

        public event ImagingProgressCallback<IImageDecoder>? Progress
        {
            add
            {
                if (value == null)
                    return;

                lock (_progressMutex)
                {
                    _progress += value;
                    if (_readProgress == null)
                    {
                        _readProgress = (p, r) => _progress?.Invoke(this, p, r?.ToMGRect());
                        ReadState.Progress += _readProgress;
                    }
                }
            }
            remove
            {
                if (value == null)
                    return;

                lock (_progressMutex)
                {
                    _progress -= value;
                    if (_progress == null && _readProgress != null)
                    {
                        ReadState.Progress -= _readProgress;
                        _readProgress = null;
                    }
                }
            }
        }

        public IImagingConfig ImagingConfig { get; }
        public TOptions DecoderOptions { get; }
        public ImageBinReader Reader { get; }
        public ReadState ReadState { get; }

        public bool IsDisposed { get; private set; }

        public Image? CurrentImage { get; private set; }
        public VectorType? SourcePixelType { get; private set; } // TODO: move into CurrentImage metadata
        public VectorType? TargetPixelType { get; set; }

        public int FrameIndex { get; protected set; }

        DecoderOptions IImageDecoder.DecoderOptions => DecoderOptions;
        CoderOptions IImageCoder.CoderOptions => DecoderOptions;

        public virtual bool CanReportProgressRectangle => false;
        public abstract ImageFormat Format { get; }

        public StbImageDecoderBase(
            IImagingConfig config, Stream stream, TOptions decoderOptions)
        {
            ImagingConfig = config ?? throw new ArgumentNullException(nameof(config));
            DecoderOptions = decoderOptions ?? throw new ArgumentNullException(nameof(decoderOptions));

            byte[] buffer = RecyclableMemoryManager.Default.GetBlock();
            Reader = new ImageBinReader(stream, buffer);

            ReadState = new ReadState()
            {
                StateReadyCallback = OnStateReady,
                OutputPixelLineCallback = OnOutputPixelLine,
                OutputPixelCallback = OnOutputPixel
            };
        }

        public void Decode(CancellationToken cancellationToken = default)
        {
            Reader.CancellationToken = cancellationToken;
            ReadState.CancellationToken = cancellationToken;

            Read();

            FrameIndex++;
        }

        protected abstract void Read();

        private void OnStateReady(ReadState state)
        {
            SourcePixelType = GetVectorType(state);
            var dstType = TargetPixelType ?? SourcePixelType;
            var size = new Size(state.Width, state.Height);

            if (DecoderOptions.ClearImageMemory)
            {
                CurrentImage = Image.Create(dstType, size);
            }
            else
            {
                CurrentImage = Image.CreateUninitialized(dstType, size);
            }

            _convertPixels = Image.GetConvertPixelsDelegate(SourcePixelType, dstType);

            _progress?.Invoke(this, 0, null);
        }

        private void AssertValidStateForOutput()
        {
            if (SourcePixelType == null)
                throw new InvalidOperationException("Missing source pixel type.");

            if (CurrentImage == null)
                throw new InvalidOperationException("Missing image buffer.");
        }

        private void OnOutputPixelLineContiguous(
            AddressingMajor addressing, int line, int start, ReadOnlySpan<byte> pixels)
        {
            if (SourcePixelType == CurrentImage!.PixelType)
            {
                if (addressing == AddressingMajor.Row)
                    CurrentImage.SetPixelByteRow(start, line, pixels);
                else if (addressing == AddressingMajor.Column)
                    CurrentImage.SetPixelByteColumn(start, line, pixels);
                else
                    throw new ArgumentOutOfRangeException(nameof(addressing));
            }
            else
            {
                int dstElementSize = CurrentImage!.PixelType.ElementSize;

                if (addressing == AddressingMajor.Row)
                {
                    var dstSpan = CurrentImage.GetPixelByteRowSpan(line)[(start * dstElementSize)..];
                    _convertPixels!.Invoke(pixels, dstSpan);
                }
                else if (addressing == AddressingMajor.Column)
                {
                    throw new NotImplementedException();
                    //CurrentImage.SetPixelByteColumn(start, line, pixels);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(addressing));
                }
            }
        }

        [SkipLocalsInit]
        private void OnOutputPixelLine(
            ReadState state, AddressingMajor addressing,
            int line, int start, int spacing, ReadOnlySpan<byte> pixelData)
        {
            if (spacing == 0)
                throw new ArgumentOutOfRangeException(nameof(spacing));

            AssertValidStateForOutput();

            if (_progress != null)
            {
                int pline = state.Orientation.HasFlag(ImageOrientation.BottomToTop) ? (state.Height - line) : line;

                _progress.Invoke(
                    this,
                    pline / (float)state.Height,
                    new Rectangle(start, line, state.Width - start, 1));
            }

            if (spacing == 1)
            {
                OnOutputPixelLineContiguous(addressing, line, start, pixelData);
                return;
            }

            int dstElementSize = CurrentImage!.PixelType.ElementSize;

            if (SourcePixelType == CurrentImage.PixelType)
            {
                if (addressing == AddressingMajor.Row)
                {
                    var imageRow = CurrentImage.GetPixelByteRowSpan(line)[(start * dstElementSize)..];
                    for (int x = 0; x < pixelData.Length; x += dstElementSize)
                    {
                        var src = pixelData.Slice(x, dstElementSize);
                        var dst = imageRow.Slice(x * spacing, dstElementSize);
                        src.CopyTo(dst);
                    }
                }
                else if (addressing == AddressingMajor.Column)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(addressing));
                }
            }
            else
            {

                if (addressing == AddressingMajor.Row)
                {
                    Span<byte> imageRow = CurrentImage.GetPixelByteRowSpan(line)[(start * dstElementSize)..];
                    Span<byte> buffer = stackalloc byte[4096];
                    int srcElementSize = SourcePixelType!.ElementSize;
                    int bufferCapacity = buffer.Length / dstElementSize;
                    int pixelCount = pixelData.Length / srcElementSize;
                    int dstOffset = 0;

                    do
                    {
                        int count = pixelCount > bufferCapacity ? bufferCapacity : pixelCount;
                        var slice = pixelData.Slice(0, count * srcElementSize);

                        _convertPixels!.Invoke(slice, buffer);

                        int bufOffset = 0;
                        for (int x = 0; x < slice.Length; x += srcElementSize)
                        {
                            var src = buffer.Slice(bufOffset, dstElementSize);
                            var dst = imageRow.Slice(dstOffset, dstElementSize);
                            src.CopyTo(dst);

                            bufOffset += dstElementSize;
                            dstOffset += dstElementSize * spacing;
                        }

                        pixelData = pixelData[slice.Length..];
                        pixelCount -= count;

                    } while (!pixelData.IsEmpty);
                }
                else if (addressing == AddressingMajor.Column)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(addressing));
                }
            }
        }

        private void OnOutputPixel(ReadState state, int x, int y, ReadOnlySpan<byte> pixel)
        {
            AssertValidStateForOutput();

            if (SourcePixelType == CurrentImage!.PixelType)
            {
                CurrentImage.SetPixelByteRow(x, y, pixel);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static VectorType GetVectorType(ReadState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            return StbImageInfoDetectorBase.GetVectorType(state.OutComponents, state.OutDepth);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (!Reader.IsDisposed)
                {
                    RecyclableMemoryManager.Default.ReturnBlock(Reader.Buffer);
                    Reader.Dispose();
                }

                IsDisposed = true;
            }
        }

        ~StbImageDecoderBase()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

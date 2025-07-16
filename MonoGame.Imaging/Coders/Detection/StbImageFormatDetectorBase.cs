using System;
using MonoGame.Imaging.Coders.Decoding;

namespace MonoGame.Imaging.Coders.Detection
{
    public abstract class StbImageFormatDetectorBase : IImageFormatDetector
    {
        public abstract ImageFormat Format { get; }
        public abstract int HeaderSize { get; }

        public CoderOptions CoderOptions => CoderOptions.Default;

        protected abstract bool TestFormat(IImagingConfig config, ReadOnlySpan<byte> header);

        public ImageFormat? DetectFormat(IImagingConfig config, ReadOnlySpan<byte> header)
        {
            if (TestFormat(config, header))
                return Format;
            return null;
        }
    }
}

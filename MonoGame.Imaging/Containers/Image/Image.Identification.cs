using System;
using System.IO;
using System.Threading;
using MonoGame.Imaging.Coders.Decoding;

namespace MonoGame.Imaging
{
    public partial class Image
    {
        #region DetectFormat

        public static ImageFormat? DetectFormat(IImagingConfig config, ReadOnlySpan<byte> header)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            foreach (IImageFormatDetector formatDetector in config.GetFormatDetectors())
            {
                if (header.Length < formatDetector.HeaderSize)
                    continue;

                ImageFormat? format = formatDetector.DetectFormat(config, header);
                if (format != null)
                    return format;
            }
            return null;
        }

        public static ImageFormat? DetectFormat(ReadOnlySpan<byte> header)
        {
            return DetectFormat(ImagingConfig.Default, header);
        }

        #endregion

        #region Identify

        public static ImageInfo Identify(
            IImagingConfig config, Stream stream, CancellationToken cancellationToken = default)
        {
            using var prefixedStream = config.CreateStreamWithHeaderPrefix(stream);
            Memory<byte> prefix = prefixedStream.GetPrefix(cancellationToken);

            ImageFormat? format = DetectFormat(config, prefix.Span) ?? throw new UnknownImageFormatException();

            var infoDetector = config.GetInfoDetector(format);
            return infoDetector.Identify(config, prefixedStream, cancellationToken);
        }

        public static ImageInfo Identify(Stream stream, CancellationToken cancellationToken = default)
        {
            return Identify(ImagingConfig.Default, stream, cancellationToken);
        }

        #endregion
    }
}

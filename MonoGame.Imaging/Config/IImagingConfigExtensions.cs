using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MonoGame.Framework.IO;
using MonoGame.Imaging.Coders.Decoding;
using MonoGame.Imaging.Coders.Encoding;
using MonoGame.Imaging.Config.Providers;

namespace MonoGame.Imaging
{
    public static class IImagingConfigExtensions
    {
        public static T GetModule<T>(this IImagingConfig config)
            where T : class
        {
            if (!config.TryGetModule(out T? module) || module == null)
                throw new MissingImagingModuleException(typeof(T));
            return module;
        }

        #region [Try]GetEncoder

        public static bool TryCreateEncoder(
            this IImagingConfig config,
            Stream stream,
            ImageFormat format,
            EncoderOptions? encoderOptions,
            [MaybeNullWhen(false)] out IImageEncoder encoder)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var provider = config.GetModule<ImageCoderProvider<IImageEncoder>>();
            var factory = provider.GetFactory(format);
            encoder = factory.Invoke(stream, encoderOptions) ?? throw new ImagingException("Coder factory returned null.");
            return true;
        }

        public static IImageEncoder CreateEncoder(
            this IImagingConfig config,
            Stream stream,
            ImageFormat format,
            EncoderOptions? encoderOptions)
        {
            if (!TryCreateEncoder(config, stream, format, encoderOptions, out var encoder))
                throw new MissingEncoderException(format);
            return encoder;
        }

        #endregion

        #region [Try]GetDecoder

        public static bool TryCreateDecoder(
            this IImagingConfig config,
            Stream stream,
            ImageFormat format,
            DecoderOptions? decoderOptions,
            [MaybeNullWhen(false)] out IImageDecoder decoder)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var provider = config.GetModule<ImageCoderProvider<IImageDecoder>>();
            var factory = provider.GetFactory(format);
            decoder = factory.Invoke(stream, decoderOptions) ?? throw new ImagingException("Coder factory returned null.");
            return true;
        }

        public static IImageDecoder CreateDecoder(
            this IImagingConfig config,
            Stream stream,
            ImageFormat format,
            DecoderOptions? decoderOptions)
        {
            if (!TryCreateDecoder(config, stream, format, decoderOptions, out var decoder))
                throw new MissingDecoderException(format);
            return decoder;
        }

        #endregion

        #region [Try]GetInfoDetector

        public static bool TryGetInfoDetector(
            this IImagingConfig config,
            ImageFormat format,
            [MaybeNullWhen(false)] out IImageInfoDetector infoDetector)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var provider = config.GetModule<ImagingInstanceProvider<IImageInfoDetector>>();
            return provider.TryGetValue(format, out infoDetector);
        }

        public static IImageInfoDetector GetInfoDetector(this IImagingConfig config, ImageFormat format)
        {
            if (!TryGetInfoDetector(config, format, out var decoder))
                throw new MissingDecoderException(format);
            return decoder;
        }

        #endregion

        public static int GetMaxHeaderSize(this IImagingConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var provider = config.GetModule<ImagingInstanceProvider<IImageFormatDetector>>();
            return provider.GetMaxHeaderSize();
        }

        public static IEnumerable<IImageFormatDetector> GetFormatDetectors(this IImagingConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var provider = config.GetModule<ImagingInstanceProvider<IImageFormatDetector>>();
            return provider.Values;
        }

        public static PrefixedStream CreateStreamWithHeaderPrefix(
            this IImagingConfig config, Stream stream)
        {
            int readAhead = GetMaxHeaderSize(config);
            return new PrefixedStream(stream, readAhead, leaveOpen: true);
        }
    }
}

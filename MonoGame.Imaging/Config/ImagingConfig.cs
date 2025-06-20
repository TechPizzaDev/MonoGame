using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MonoGame.Imaging.Coders;
using MonoGame.Imaging.Coders.Decoding;
using MonoGame.Imaging.Coders.Encoding;
using MonoGame.Imaging.Coders.Formats.Bmp;
using MonoGame.Imaging.Coders.Formats.Gif;
using MonoGame.Imaging.Coders.Formats.Jpeg;
using MonoGame.Imaging.Coders.Formats.Png;
using MonoGame.Imaging.Coders.Formats.Tga;
using MonoGame.Imaging.Config.Providers;

namespace MonoGame.Imaging
{
    public class ImagingConfig : IImagingConfig
    {
        private readonly Dictionary<Type, object> _modules = new();

        /// <summary>
        /// Gets the default <see cref="ImagingConfig"/>,
        /// often used for methods that don't take it as an argument.
        /// </summary>
        public static ImagingConfig Default { get; } = new ImagingConfig();

        public ImagingConfig()
        {
            _modules = new Dictionary<Type, object>();

            // TODO: improve this

            var decoders = new ImageCoderProvider<IImageDecoder>();
            decoders.TryAddFactory(ImageFormat.Bmp, (stream, options) => 
                new BmpImageDecoder(this, stream, CheckCoderOptions<DecoderOptions>(options)));
            
            decoders.TryAddFactory(ImageFormat.Tga, (stream, options) => 
                new TgaImageDecoder(this, stream, CheckCoderOptions<DecoderOptions>(options)));
            
            decoders.TryAddFactory(ImageFormat.Png, (stream, options) => 
                new PngImageDecoder(this, stream, CheckCoderOptions<DecoderOptions>(options)));
            
            decoders.TryAddFactory(ImageFormat.Jpeg, (stream, options) => 
                new JpegImageDecoder(this, stream, CheckCoderOptions<DecoderOptions>(options)));


            var encoders = new ImageCoderProvider<IImageEncoder>();
            encoders.TryAddFactory(ImageFormat.Bmp, (stream, options) =>
                new BmpImageEncoder(this, stream, CheckCoderOptions<EncoderOptions>(options)));

            encoders.TryAddFactory(ImageFormat.Tga, (stream, options) =>
                new TgaImageEncoder(this, stream, CheckCoderOptions<TgaEncoderOptions>(options)));

            encoders.TryAddFactory(ImageFormat.Png, (stream, options) =>
                new PngImageEncoder(this, stream, CheckCoderOptions<PngEncoderOptions>(options)));

            encoders.TryAddFactory(ImageFormat.Jpeg, (stream, options) =>
                new JpegImageEncoder(this, stream, CheckCoderOptions<JpegEncoderOptions>(options)));

            encoders.TryAddFactory(ImageFormat.Gif, (stream, options) =>
                new GifImageEncoder(this, stream, CheckCoderOptions<EncoderOptions>(options)));


            var formatDetectors = new ImagingInstanceProvider<IImageFormatDetector>();
            formatDetectors.TryAdd(ImageFormat.Bmp, new BmpImageFormatDetector());
            formatDetectors.TryAdd(ImageFormat.Tga, new TgaImageFormatDetector());
            formatDetectors.TryAdd(ImageFormat.Png, new PngImageFormatDetector());
            formatDetectors.TryAdd(ImageFormat.Jpeg, new JpegImageFormatDetector());


            _modules.Add(decoders.GetType(), decoders);
            _modules.Add(encoders.GetType(), encoders);
            _modules.Add(formatDetectors.GetType(), formatDetectors);
        }

        [return: NotNullIfNotNull("options")]
        public static TOptions? CheckCoderOptions<TOptions>(CoderOptions? options)
           where TOptions : CoderOptions
        {
            if (options == null)
                return null;

            if (typeof(TOptions).IsAssignableFrom(options.GetType()))
                return (TOptions)options;

            throw new ArgumentException("", nameof(options));
        }

        public bool TryGetModule<T>([MaybeNullWhen(false)] out T value) where T : class
        {
            if (_modules.TryGetValue(typeof(T), out object? obj))
            {
                value = (T) obj;
                return true;
            }
            value = default;
            return false;
        }
    }
}

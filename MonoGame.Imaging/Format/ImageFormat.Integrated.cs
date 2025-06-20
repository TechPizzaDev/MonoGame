using System;
using System.Collections.Generic;
using MonoGame.Framework.Collections;

namespace MonoGame.Imaging
{
    public partial class ImageFormat
    {
        private static readonly HashSet<ImageFormat> _integratedFormats = new();

        #region Getters (and Initializers)

        /// <summary>
        /// Gets the "Portable Network Graphics" (PNG) format.
        /// </summary>
        public static ImageFormat Png { get; } = AddIntegrated(
            "Portable Network Graphics", "PNG",
            new[] { "image/png" },
            new[] { ".png" });

        /// <summary> 
        /// Gets the "Joint Photographic Experts Group" (JPEG) format. 
        /// </summary>
        public static ImageFormat Jpeg { get; } = AddIntegrated(
            "Joint Photographic Experts Group", "JPEG",
            new[] { "image/jpeg", "image/pjpeg" },
            new[] { ".jpeg", ".jpg", ".jfif", ".jpe", ".jif" });

        /// <summary>
        /// Gets the "Graphics Interchange Format" (GIF).
        /// </summary>
        public static AnimatedImageFormat Gif { get; } = new AnimatedImageFormat(
            "Graphics Interchange Format", "GIF",
            new HashSet<string> { "image/gif" }.AsReadOnly(),
            new HashSet<string> { ".gif" }.AsReadOnly(),
            TimeSpan.FromSeconds(0.01));

        /// <summary>
        /// Gets the "Bitmap" (BMP) format.
        /// </summary>
        public static ImageFormat Bmp { get; } = AddIntegrated(
            "Bitmap", "BMP",
            new[] { "image/bmp", "image/x-bmp", "image/x-windows-bmp" },
            new[] { ".bmp", ".bm", ".dip" });

        /// <summary>
        /// Gets the "Truevision Graphics Adapter" (TGA) format.
        /// </summary>
        public static ImageFormat Tga { get; } = AddIntegrated(
            "Truevision Graphics Adapter", "TGA",
            new[] { "image/x-tga", "image/x-targa" },
            new[] { ".tga", ".icb", ".vda", ".vst" });

        /// <summary>
        /// Gets the "RGBE" format (also known as "Radiance HDR").
        /// </summary>
        public static ImageFormat Rgbe { get; } = AddIntegrated(
            "Radiance HDR", "RGBE",
            new[] { "image/vnd.radiance", "image/x-hdr" },
            new[] { ".hdr", ".rgbe" });

        /// <summary>
        /// Gets the "PhotoShop Document" (PSD) format.
        /// </summary>
        public static ImageFormat Psd { get; } = new LayeredImageFormat(
            "PhotoShop Document", "PSD",
            new HashSet<string> { "image/vnd.adobe.photoshop", "application/x-photoshop" }.AsReadOnly(),
            new HashSet<string> { ".psd" }.AsReadOnly());

        #endregion

        static ImageFormat()
        {
            AddFormat(Gif);
            _integratedFormats.Add(Gif);

            AddFormat(Psd);
            _integratedFormats.Add(Psd);
        }

        /// <summary>
        /// Gets whether the format comes with the imaging library.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
        public static bool IsIntegrated(ImageFormat format)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            return _integratedFormats.Contains(format);
        }

        private static ImageFormat AddIntegrated(
            string fullName, string name, string[] mimeTypes, string[] extensions)
        {
            var mimeSet = new ReadOnlySet<string>(mimeTypes, StringComparer.OrdinalIgnoreCase);
            var extensionSet = new ReadOnlySet<string>(extensions, StringComparer.OrdinalIgnoreCase);

            var format = new ImageFormat(
                fullName, name,
                mimeTypes[0], extensions[0], mimeSet, extensionSet);

            _integratedFormats.Add(format);
            AddFormat(format);
            return format;
        }
    }
}

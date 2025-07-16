using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            ["image/png"],
            [".png"]);

        /// <summary> 
        /// Gets the "Joint Photographic Experts Group" (JPEG) format. 
        /// </summary>
        public static ImageFormat Jpeg { get; } = AddIntegrated(
            "Joint Photographic Experts Group", "JPEG",
            ["image/jpeg", "image/pjpeg"],
            [".jpeg", ".jpg", ".jfif", ".jpe", ".jif"]);

        /// <summary>
        /// Gets the "Graphics Interchange Format" (GIF).
        /// </summary>
        public static ImageFormat Gif { get; } = AddIntegrated(new AnimatedImageFormat(
            "Graphics Interchange Format", "GIF",
            "image/gif",
            ".gif",
            [], [],
            TimeSpan.FromSeconds(0.01)));

        /// <summary>
        /// Gets the "Bitmap" (BMP) format.
        /// </summary>
        public static ImageFormat Bmp { get; } = AddIntegrated(
            "Bitmap", "BMP",
            ["image/bmp", "image/x-bmp", "image/x-windows-bmp"],
            [".bmp", ".bm", ".dip"]);

        /// <summary>
        /// Gets the "Truevision Graphics Adapter" (TGA) format.
        /// </summary>
        public static ImageFormat Tga { get; } = AddIntegrated(
            "Truevision Graphics Adapter", "TGA",
            ["image/x-tga", "image/x-targa"],
            [".tga", ".icb", ".vda", ".vst"]);

        /// <summary>
        /// Gets the "RGBE" format (also known as "Radiance HDR").
        /// </summary>
        public static ImageFormat Rgbe { get; } = AddIntegrated(
            "Radiance HDR", "RGBE",
            ["image/vnd.radiance", "image/x-hdr"],
            [".hdr", ".rgbe"]);

        /// <summary>
        /// Gets the "PhotoShop Document" (PSD) format.
        /// </summary>
        public static ImageFormat Psd { get; } = AddIntegrated(new LayeredImageFormat(
            "PhotoShop Document", "PSD",
            "image/vnd.adobe.photoshop",
            ".psd",
            ["application/x-photoshop"], []));

        #endregion

        static ImageFormat()
        {
            foreach (ImageFormat format in _integratedFormats)
            {
                bool added = _formats.Add(format);
                Debug.Assert(added);

                AddToDictionary(_byMimeType, format.MimeTypes, format);
                AddToDictionary(_byExtension, format.Extensions, format);
            }
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
            return AddIntegrated(new ImageFormat(
                fullName, name, mimeTypes[0], extensions[0], mimeTypes, extensions));
        }

        private static ImageFormat AddIntegrated(ImageFormat format)
        {
            _integratedFormats.Add(format);
            return format;
        }
    }
}

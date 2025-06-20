using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MonoGame.Framework;
using MonoGame.Framework.Collections;
using MonoGame.Imaging.Attributes;
using FormatList = MonoGame.Framework.Collections.CachedReadOnlyList<MonoGame.Imaging.ImageFormat>;

namespace MonoGame.Imaging
{
    using FormatDictionary = Dictionary<string, FormatList>;

    // TODO: add coder priority so the user can implement
    // an alternative coder in place of an existing one

    [DebuggerDisplay("{ToString(),nq}")]
    public partial class ImageFormat : IImageFormatAttribute
    {
        private static object RegistrationMutex { get; } = new object();

        private static HashSet<ImageFormat> _formats = new();
        private static FormatDictionary _byMimeType = new FormatDictionary(StringComparer.OrdinalIgnoreCase);
        private static FormatDictionary _byExtension = new FormatDictionary(StringComparer.OrdinalIgnoreCase);

        #region Properties

        /// <summary>
        /// Gets the full name of the format.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the short name of the format, often used as the extension.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets the primary MIME type associated with the format.
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// Gets the primary file extension associated with the format.
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Gets MIME types associated with the format.
        /// </summary>
        public ReadOnlySet<string> MimeTypes { get; }

        /// <summary>
        /// Gets file extensions associated with the format.
        /// </summary>
        public ReadOnlySet<string> Extensions { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        /// <param name="fullName">The full name of the format.</param>
        /// <param name="shortName">The short name of the format.</param>
        public ImageFormat(
            string fullName,
            string shortName, 
            string primaryMimeType, 
            string primaryExtension,
            IReadOnlySet<string> mimeTypes,
            IReadOnlySet<string> extensions)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            ShortName = shortName ?? throw new ArgumentNullException(nameof(shortName));
            MimeType = primaryMimeType ?? throw new ArgumentNullException(nameof(primaryMimeType));
            Extension = ValidateExtension(primaryExtension) ?? throw new ArgumentNullException(nameof(primaryExtension));
            
            if (mimeTypes == null) throw new ArgumentNullException(nameof(mimeTypes));
            if (mimeTypes.Count == 0) throw new ArgumentEmptyException(nameof(mimeTypes));
            if (!mimeTypes.Contains(primaryMimeType))
                throw new ArgumentException("The set doesn't contain the primary MIME type.", nameof(mimeTypes));

            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
            if (extensions.Count == 0) throw new ArgumentEmptyException(nameof(extensions));
            if (!extensions.Contains(primaryExtension))
                throw new ArgumentException("The set doesn't contain the primary extension.", nameof(mimeTypes));

            MimeTypes = new ReadOnlySet<string>(mimeTypes, StringComparer.OrdinalIgnoreCase);
            Extensions = new ReadOnlySet<string>(extensions, StringComparer.OrdinalIgnoreCase);
        }

        public ImageFormat(
            string fullName,
            string shortName,
            string mimeType,
            string extension) :
            this(
                fullName,
                shortName,
                mimeType,
                extension,
                new ReadOnlySet<string>(new[] { mimeType }, StringComparer.OrdinalIgnoreCase),
                new ReadOnlySet<string>(new[] { extension }, StringComparer.OrdinalIgnoreCase))
        {
        }

        public ImageFormat(
            string fullName,
            string shortName,
            IReadOnlySet<string> mimeTypes,
            IReadOnlySet<string> extensions) :
            this(
                fullName, shortName,
                mimeTypes.First(),
                extensions.First(),
                mimeTypes,
                extensions)
        {
        }

        #endregion

        private static string? ValidateExtension(string extension)
        {
            if (extension == null)
                return null;

            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentEmptyException(nameof(extension));

            if (extension[0] == '.')
                return extension;
            return "." + extension;
        }

        #region Custom ImageFormats

        public static void AddFormat(ImageFormat format)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            lock (RegistrationMutex)
            {
                if (!_formats.Add(format))
                    throw new ArgumentException(
                        "The format has already been added.", nameof(format));

                void AddToDictionary(FormatDictionary dictionary, ReadOnlySet<string> keys)
                {
                    foreach (string key in keys)
                    {
                        if (!dictionary.TryGetValue(key, out var list))
                        {
                            list = FormatList.Create();
                            // Use ToLower() to "sanitize" keys, shouldn't change functionality 
                            // as the format dictionaries should be case-insensitive.
                            dictionary.Add(key.ToLower(), list);
                        }
                        list.Source.Add(format);
                    }
                }

                AddToDictionary(_byMimeType, format.MimeTypes);
                AddToDictionary(_byExtension, format.Extensions);
            }
        }

        #endregion

        #region ImageFormat Getters

        #region [Try]GetByMimeType

        public static bool TryGetByMimeType(string mimeType, out ReadOnlyCollection<ImageFormat>? formats)
        {
            if (_byMimeType.TryGetValue(mimeType, out var list))
            {
                formats = list.ReadOnly;
                return true;
            }
            formats = default;
            return false;
        }

        #endregion

        #region [Try]GetByExtension

        public static bool TryGetByExtension(string extension, out ReadOnlyCollection<ImageFormat>? formats)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            if (extension.Length > 0 && !extension.StartsWith("."))
                extension = "." + extension;

            if (_byExtension.TryGetValue(extension, out var list))
            {
                formats = list.ReadOnly;
                return true;
            }
            formats = default;
            return false;
        }

        public static ReadOnlyCollection<ImageFormat> GetByExtension(string extension)
        {
            if (!TryGetByExtension(extension, out var formats))
                throw new KeyNotFoundException(
                    $"No image formats with extension '{extension}' are defined.");

            return formats!;
        }

        public static bool TryGetByPath(string path, out ReadOnlyCollection<ImageFormat>? formats)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            string extension = Path.GetExtension(path);
            return TryGetByExtension(extension, out formats);
        }

        public static ReadOnlyCollection<ImageFormat> GetByPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            string extension = Path.GetExtension(path);
            return GetByExtension(extension);
        }

        #endregion

        #endregion

        public override string ToString()
        {
            return $"{{Name: \"{FullName}\", Extension: \"{Extension}\", MIME: \"{MimeType}\"}}";
        }
    }
}
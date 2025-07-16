using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using MonoGame.Imaging.Attributes;

namespace MonoGame.Imaging
{
    // TODO: add coder priority so the user can implement
    // an alternative coder in place of an existing one

    [DebuggerDisplay("{ToString(),nq}")]
    public partial class ImageFormat : IImageFormatAttribute
    {
        private static object RegistrationMutex { get; } = new object();

        private static HashSet<ImageFormat> _formats = new();
        private static Dictionary<string, ImmutableArray<ImageFormat>> _byMimeType = new();
        private static Dictionary<string, ImmutableArray<ImageFormat>> _byExtension = new();

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
        public string? MimeType { get; }

        /// <summary>
        /// Gets the primary file extension associated with the format.
        /// </summary>
        public string? Extension { get; }

        /// <summary>
        /// Gets MIME types associated with the format.
        /// </summary>
        public IReadOnlySet<string> MimeTypes { get; }

        /// <summary>
        /// Gets file extensions associated with the format.
        /// </summary>
        public IReadOnlySet<string> Extensions { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// </summary>
        /// <param name="fullName">The full name of the format.</param>
        /// <param name="shortName">The short name of the format.</param>
        public ImageFormat(
            string fullName,
            string shortName,
            string? mimeType,
            string? extension,
            IEnumerable<string> mimeTypes,
            IEnumerable<string> extensions)
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            ShortName = shortName ?? throw new ArgumentNullException(nameof(shortName));

            MimeType = mimeType;
            Extension = Path.GetExtension(extension);
            if (Extension?.Length == 0)
                throw new ArgumentException("Not an extension.", nameof(extension));

            MimeTypes = FreezeSet(mimeTypes, MimeType);
            Extensions = FreezeSet(extensions, Extension);
        }

        private static IReadOnlySet<string> FreezeSet(IEnumerable<string> values, string? primary)
        {
            HashSet<string> set = new(values);
            if (primary != null)
            {
                set.Add(primary);
            }
            return set.ToFrozenSet();
        }

        #endregion

        #region Custom ImageFormats

        public static void AddFormat(ImageFormat format)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            lock (RegistrationMutex)
            {
                if (!_formats.Add(format))
                    throw new ArgumentException("The format has already been added.", nameof(format));

                AddToDictionary(_byMimeType, format.MimeTypes, format);
                AddToDictionary(_byExtension, format.Extensions, format);
            }
        }

        private static void AddToDictionary(
            Dictionary<string, ImmutableArray<ImageFormat>> dictionary,
            IReadOnlySet<string> keys,
            ImageFormat format)
        {
            foreach (string key in keys)
            {
                var array = dictionary.GetValueOrDefault(key, []);
                if (!array.Contains(format))
                {
                    dictionary[key] = array.Add(format);
                }
            }
        }

        #endregion

        #region ImageFormat Getters

        #region [Try]GetByMimeType

        public static bool TryGetByMimeType(string mimeType, out ImmutableArray<ImageFormat> formats)
        {
            return _byMimeType.TryGetValue(mimeType, out formats);
        }

        #endregion

        #region [Try]GetByExtension

        public static bool TryGetByExtension(string extension, out ImmutableArray<ImageFormat> formats)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            
            return _byExtension.TryGetValue(extension, out formats);
        }

        public static ImmutableArray<ImageFormat> GetByExtension(string extension)
        {
            if (!TryGetByExtension(extension, out var formats))
            {
                throw new KeyNotFoundException(
                    $"No image formats with extension '{extension}' are defined.");
            }
            return formats;
        }

        public static bool TryGetByPath(string path, out ImmutableArray<ImageFormat> formats)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            string extension = Path.GetExtension(path);
            return TryGetByExtension(extension, out formats);
        }

        public static ImmutableArray<ImageFormat> GetByPath(string path)
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
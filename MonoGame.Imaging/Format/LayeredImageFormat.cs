using System.Collections.Generic;
using MonoGame.Imaging.Attributes.Format;

namespace MonoGame.Imaging
{
    public class LayeredImageFormat : ImageFormat, ILayeredFormatAttribute
    {
        public LayeredImageFormat(
            string fullName, 
            string shortName,
            string? mimeType,
            string? extension,
            IEnumerable<string> mimeTypes,
            IEnumerable<string> extensions) :
            base(fullName, shortName, mimeType, extension, mimeTypes, extensions)
        {
        }
    }
}

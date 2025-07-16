using System;
using System.Collections.Generic;
using MonoGame.Imaging.Attributes.Format;

namespace MonoGame.Imaging
{
    public class AnimatedImageFormat : ImageFormat, IAnimatedFormatAttribute
    {
        public TimeSpan MinimumAnimationDelay { get; }

        public AnimatedImageFormat(
            string fullName, 
            string shortName,
            string? mimeType, 
            string? extension,
            IEnumerable<string> mimeTypes,
            IEnumerable<string> extensions,
            TimeSpan minimumAnimationDelay) :
            base(fullName, shortName, mimeType, extension, mimeTypes, extensions)
        {
            MinimumAnimationDelay = minimumAnimationDelay.Duration();
        }
    }
}

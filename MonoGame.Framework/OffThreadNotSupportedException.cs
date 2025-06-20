using System;

namespace MonoGame.Framework
{
    public class OffThreadNotSupportedException : NotSupportedException
    {
        public OffThreadNotSupportedException() : base(FrameworkResources.OffThreadNotSupported)
        {
        }

        public OffThreadNotSupportedException(string? message) : 
            base(message ?? FrameworkResources.OffThreadNotSupported)
        {
        }

        public OffThreadNotSupportedException(string? message, Exception? inner) :
            base(message ?? FrameworkResources.OffThreadNotSupported, inner)
        {
        }
    }
}
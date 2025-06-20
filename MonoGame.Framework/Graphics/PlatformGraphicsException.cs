using System;

namespace MonoGame.Framework.Graphics
{
    public class PlatformGraphicsException : Exception
    {
        public PlatformGraphicsException() 
        {
        }

        public PlatformGraphicsException(string? message) : base(message) 
        { 
        }

        public PlatformGraphicsException(string? message, Exception? innerException) : base(message, innerException) 
        { 
        }
    }
}

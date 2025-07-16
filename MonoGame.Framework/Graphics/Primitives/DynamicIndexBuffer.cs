// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Graphics
{
    public sealed class DynamicIndexBuffer : IndexBuffer
    {
        /// <summary>
        /// Special offset used internally by GraphicsDevice.DrawUserXXX() methods.
        /// </summary>
        internal int UserOffset;

        public bool IsContentLost => false;

        public DynamicIndexBuffer(
            GraphicsDevice graphicsDevice, 
            IndexElementType elementType,
            int capacity,
            BufferUsage bufferUsage) :
            base(graphicsDevice, elementType, capacity, bufferUsage, true)
        {
        }
    }
}


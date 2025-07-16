// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Graphics
{
    public sealed class DynamicVertexBuffer : VertexBuffer
    {
        /// <summary>
        /// Special offset used internally by GraphicsDevice.DrawUserXXX() methods.
        /// </summary>
        internal int UserOffset;

        public bool IsContentLost => false;

        public DynamicVertexBuffer(
            GraphicsDevice graphicsDevice,
            VertexDeclaration vertexDeclaration, 
            int capacity, 
            BufferUsage bufferUsage)
            : base(graphicsDevice, vertexDeclaration, capacity, bufferUsage, true)
        {
        }
        
        public DynamicVertexBuffer(
            GraphicsDevice graphicsDevice,
            Type vertexType, 
            int capacity,
            BufferUsage bufferUsage)
            : base(graphicsDevice, VertexDeclaration.FromType(vertexType), capacity, bufferUsage, true)
        {
        }
    }
}


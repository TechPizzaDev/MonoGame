// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Graphics
{
    public partial class VertexBuffer : BufferBase
    {
        /// <summary>
        /// The vertex declaration of the vertex type that this buffer contains.
        /// </summary>
        public VertexDeclaration VertexDeclaration { get; protected set; }

        #region Constructors

        protected VertexBuffer(
            GraphicsDevice graphicsDevice,
            VertexDeclaration vertexDeclaration,
            int capacity,
            BufferUsage bufferUsage,
            bool isDynamic) :
            base(graphicsDevice, capacity, bufferUsage, isDynamic)
        {
            vertexDeclaration.BindToGraphicsDevice(graphicsDevice);

            VertexDeclaration = vertexDeclaration;

            if (IsValidThreadContext)
                PlatformConstruct();
            else
                Threading.BlockOnMainThread(PlatformConstruct);
        }

        public VertexBuffer(
            GraphicsDevice graphicsDevice,
            VertexDeclaration vertexDeclaration,
            int capacity,
            BufferUsage bufferUsage) :
            this(graphicsDevice, vertexDeclaration, capacity, bufferUsage, false)
        {
        }

        public VertexBuffer(
            GraphicsDevice graphicsDevice,
            Type vertexType,
            int capacity,
            BufferUsage bufferUsage) :
            this(graphicsDevice, VertexDeclaration.FromType(vertexType), capacity, bufferUsage, false)
        {
        }

        #endregion

        #region GetData

        /// <summary>
        /// Get the vertex data from this vertex buffer with an optional buffer offset.
        /// </summary>
        /// <param name="destination">The span to be filled.</param>
        /// <param name="byteOffset">The offset to the first element in the vertex buffer in bytes.</param>
        /// <param name="elementStride">The size of how a vertex buffer element should be interpreted.</param>
        /// <remarks>
        /// Note that this pulls data from VRAM into main memory and because of that it's an expensive operation.
        /// It is often a better idea to keep a copy of the data in main memory.
        /// </remarks>
        public void GetData<T>(
            int byteOffset, Span<T> destination, int elementStride = 0)
            where T : unmanaged
        {
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new InvalidOperationException(FrameworkResources.WriteOnlyResource);

            AssertMainThread(true);

            if (elementStride == 0)
                elementStride = Unsafe.SizeOf<T>();

            int dstSize = destination.Length * Unsafe.SizeOf<T>();
            if (elementStride > dstSize)
                throw new ArgumentOutOfRangeException(
                    nameof(elementStride), "Element stride may not be larger than the given buffer size.");

            int bufferSize = Capacity * VertexDeclaration.VertexStride;
            if (elementStride > bufferSize)
                throw new ArgumentOutOfRangeException(
                    nameof(elementStride), "The vertex stride may not be larger than the buffer capacity.");

            if (byteOffset + dstSize > bufferSize)
                throw new ArgumentException(
                    "The requested range of offset and length reaches beyond the buffer.");

            PlatformGetData(byteOffset, MemoryMarshal.AsBytes(destination), elementStride);
        }

        public void GetData<T>(Span<T> destination, int elementStride = 0)
            where T : unmanaged
        {
            GetData(0, destination, elementStride);
        }

        #endregion

        #region SetData

        /// <summary>
        /// Sets the vertex buffer data.
        /// </summary>
        /// <param name="byteOffset">Offset in bytes from the beginning of the vertex buffer to start copying to.</param>
        /// <param name="source">The span of vertex data.</param>
        /// <param name="elementStride">
        /// Specifies how far apart, in bytes, elements from <paramref name="source"/> should be when 
        /// they are copied into the vertex buffer.
        /// If you specify <c>sizeof(T)</c>, elements from <paramref name="source"/> will be copied into the 
        /// vertex buffer with no padding between each element.
        /// If you specify a value greater than <c>sizeof(T)</c>, elements from <paramref name="source"/> will be copied 
        /// into the vertex buffer with padding between each element.
        /// If you specify <c>0</c> for this parameter, it will be treated as if you had specified <c>sizeof(T)</c>.
        /// With the exception of <c>0</c>, you must specify a value greater than or equal to <c>sizeof(T)</c>.
        /// </param>
        /// <param name="options">The options to use when flushing the data.</param>
        /// <remarks>
        /// Example: If you only want to set the texture coordinate component of the vertex data,
        /// you would call this method as follows (note the use of <paramref name="byteOffset"/>:
        /// <code>
        /// var texCoords = new Vector2[vertexCount];
        /// vertexBuffer.SetData(offsetInBytes: 12, texCoords);
        /// </code>
        /// </remarks>
        public void SetData<T>(
            int byteOffset,
            ReadOnlySpan<T> source,
            int elementStride = 0,
            SetDataOptions options = SetDataOptions.None)
            where T : unmanaged
        {
            if (source.IsEmpty)
                return;

            AssertMainThread(true);

            if (elementStride == 0)
                elementStride = Unsafe.SizeOf<T>();

            int srcSize = source.Length * Unsafe.SizeOf<T>();
            if (elementStride > srcSize)
                throw new ArgumentOutOfRangeException(
                    nameof(elementStride), "Element stride may not be larger than the given buffer size.");

            int bufferSize = Capacity * VertexDeclaration.VertexStride;
            if (elementStride > bufferSize)
                throw new ArgumentOutOfRangeException(
                    nameof(elementStride), "Element stride may not be larger than the vertex buffer capacity.");

            if (byteOffset + srcSize > bufferSize)
                throw new ArgumentException("The given range reaches beyond the buffer.");

            PlatformSetData(byteOffset, MemoryMarshal.AsBytes(source), elementStride, options);
            Count = srcSize / elementStride;
        }

        public void SetData<T>(
            int byteOffset,
            Span<T> source,
            int elementStride = 0,
            SetDataOptions options = SetDataOptions.None)
            where T : unmanaged
        {
            SetData(byteOffset, (ReadOnlySpan<T>)source, elementStride, options);
        }

        public void SetData<T>(
            ReadOnlySpan<T> source,
            int elementStride = 0,
            SetDataOptions options = SetDataOptions.None)
            where T : unmanaged
        {
            SetData(0, source, elementStride, options);
        }

        public void SetData<T>(
            Span<T> source,
            int elementStride = 0,
            SetDataOptions options = SetDataOptions.None)
            where T : unmanaged
        {
            SetData((ReadOnlySpan<T>)source, elementStride, options);
        }

        #endregion
    }
}
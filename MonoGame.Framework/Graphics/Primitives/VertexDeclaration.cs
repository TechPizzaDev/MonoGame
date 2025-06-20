// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MonoGame.Framework.Graphics
{
    /// <summary>
    /// Defines per-vertex data of a vertex buffer.
    /// </summary>
    /// <remarks>
    /// <see cref="VertexDeclaration"/> implements <see cref="IEquatable{T}"/> and can be used as
    /// a key in a dictionary. Two vertex declarations are considered equal if the vertices are
    /// structurally equivalent, i.e. the vertex elements and the vertex stride are identical. (The
    /// properties <see cref="GraphicsResource.Name"/> and <see cref="GraphicsResource.Tag"/> are
    /// ignored in <see cref="GetHashCode"/> and <see cref="Equals(VertexDeclaration)"/>!)
    /// </remarks>
    public partial class VertexDeclaration : GraphicsResource, IEquatable<VertexDeclaration>
    {
        // Note for future refactoring:
        // For XNA-compatibility VertexDeclaration is derived from GraphicsResource, which means it
        // has GraphicsDevice, Name, Tag and implements IDisposable. This is unnecessary in MonoGame. 
        // VertexDeclaration.GraphicsDevice is never set.
        // --> VertexDeclaration should be a lightweight immutable type. No base class, no IDisposable.
        //     (Use the internal type Data. Do not expose a constructor. Use a factory method to
        //     cache the vertex declarations.)

        private static readonly Dictionary<Data, VertexDeclaration> _vertexDeclarationCache = new();

        private readonly Data _data;

        /// <summary>
        /// Gets the vertex elements array.
        /// </summary>
        public ReadOnlyMemory<VertexElement> VertexElements => _data.Elements;

        /// <summary>
        /// Gets the size of a vertex (including padding) in bytes.
        /// </summary>
        public int VertexStride => _data.VertexStride;

        #region Constructors

        private VertexDeclaration(Data data)
        {
            _data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexDeclaration"/> class.
        /// </summary>
        /// <param name="elements">The vertex elements.</param>
        /// <exception cref="ArgumentNullException"><paramref name="elements"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentEmptyException"><paramref name="elements"/> is empty.</exception>
        public VertexDeclaration(params VertexElement[] elements) : this(GetVertexStride(elements), elements)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexDeclaration"/> class.
        /// </summary>
        /// <param name="vertexStride">The size of a vertex (including padding) in bytes.</param>
        /// <param name="elements">The vertex elements.</param>
        /// <exception cref="ArgumentNullException"><paramref name="elements"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentEmptyException"><paramref name="elements"/> is empty.</exception>
        public VertexDeclaration(int vertexStride, params VertexElement[] elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));
            if (elements.Length == 0)
                throw new ArgumentEmptyException(nameof(elements));

            lock (_vertexDeclarationCache)
            {
                var data = new Data(vertexStride, elements);
                if (!_vertexDeclarationCache.TryGetValue(data, out var vertexDeclaration))
                {
                    // Cache new vertex declaration.
                    _data = new Data(vertexStride, (VertexElement[])elements.Clone());
                    _vertexDeclarationCache[_data] = this;
                }
                else
                {
                    // Reuse existing data.
                    _data = vertexDeclaration._data;
                }
            }
        }

        #endregion

        private static int GetVertexStride(ReadOnlySpan<VertexElement> elements)
        {
            int max = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                int start = elements[i].Offset + elements[i].VertexElementFormat.GetSize();
                if (max < start)
                    max = start;
            }
            return max;
        }

        internal static VertexDeclaration GetOrCreate(int vertexStride, VertexElement[] elements)
        {
            lock (_vertexDeclarationCache)
            {
                var key = new Data(vertexStride, elements);
                if (!_vertexDeclarationCache.TryGetValue(key, out var vertexDeclaration))
                {
                    // Entries in the vertex declaration cache must be immutable.
                    // Therefore, we create a copy of the array, which the user cannot access.
                    var data = new Data(vertexStride, (VertexElement[])elements.Clone());
                    vertexDeclaration = new VertexDeclaration(data);
                    _vertexDeclarationCache[data] = vertexDeclaration;
                }
                return vertexDeclaration;
            }
        }

        /// <summary>
        /// Returns the <see cref="VertexDeclaration"/> for Type.
        /// </summary>
        /// <param name="vertexType">A value type which implements the <see cref="IVertexType"/> interface.</param>
        /// <remarks>
        /// Prefer to use <see cref="VertexDeclarationCache{T}"/> when the declaration lookup
        /// can be performed with a templated type.
        /// </remarks>
        internal static VertexDeclaration FromType(Type vertexType)
        {
            if (vertexType == null)
                throw new ArgumentNullException(nameof(vertexType));

            if (!vertexType.IsValueType)
                throw new ArgumentException("Must be value type.", nameof(vertexType));

            if (!(Activator.CreateInstance(vertexType) is IVertexType type))
                throw new ArgumentException(
                    $"{nameof(vertexType)} does not implement {nameof(IVertexType)}.", nameof(vertexType));

            var vertexDeclaration = type.VertexDeclaration;
            if (vertexDeclaration is null)
                throw new Exception(
                    $"{nameof(IVertexType)}.{nameof(IVertexType.VertexDeclaration)} may not be null.");

            return vertexDeclaration;
        }

        /// <summary>
        /// Determines whether the specified <see cref="VertexDeclaration"/> is equal to this instance.
        /// </summary>
        public bool Equals(VertexDeclaration? other) => _data.Equals(other?._data);

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        public override bool Equals(object? obj) => Equals(obj as VertexDeclaration);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode() => _data.GetHashCode();

        /// <summary>
        /// Compares two <see cref="VertexDeclaration"/> instances to determine whether they are the same.
        /// </summary>
        public static bool operator ==(VertexDeclaration a, VertexDeclaration b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="VertexDeclaration"/> instances to determine whether they are different.
        /// </summary>
        public static bool operator !=(VertexDeclaration a, VertexDeclaration b) => !(a == b);
    }
}

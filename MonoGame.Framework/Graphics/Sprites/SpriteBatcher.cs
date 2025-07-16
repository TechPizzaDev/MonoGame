using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Graphics
{
    /// <summary>
    /// This class handles the queueing of batch items into the GPU by creating the triangle tesselations
    /// that are used to draw the sprite textures. This class supports int.MaxValue number of sprites to be
    /// batched and will process them into short.MaxValue groups (strided by 6 for the number of vertices
    /// sent to the GPU). 
    /// </summary>
    internal class SpriteBatcher : IDisposable
    {
        /*
         * Note that this class is fundamental to high performance for SpriteBatch games. Please exercise
         * caution when making changes to this class.
         */

        private const int InitialBatchSize = 256;

        /// <summary>
        /// The maximum number of batch items that can be 
        /// drawn at once with <see cref="FlushQuads"/>.
        /// </summary>
        private const int MaxBatchSize = ushort.MaxValue / 4;

        private readonly GraphicsDevice _device;

        private int _itemCount;
        private SpriteBatchItem[] _batchItems;

        private SpriteQuad[]? _quadBuffer;
        private IndexBuffer? _indexBuffer;
        private DynamicVertexBuffer? _vertexBuffer;

        public bool IsDisposed { get; private set; }

        public SpriteBatcher(GraphicsDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));

            _batchItems = new SpriteBatchItem[InitialBatchSize];
            for (int i = 0; i < _batchItems.Length; i++)
                _batchItems[i] = new SpriteBatchItem();

            SetupBuffers(InitialBatchSize);
        }

        /// <summary>
        /// Calculates indices and resizes buffer capacity (up to <see cref="MaxBatchSize"/>).
        /// </summary>
        private void SetupBuffers(int itemCount)
        {
            int minVertices = itemCount * 4; // 4 vertices per item
            if (_vertexBuffer == null || minVertices > _vertexBuffer.Capacity)
            {
                _quadBuffer = new SpriteQuad[minVertices];

                _vertexBuffer?.Dispose();
                _vertexBuffer = new DynamicVertexBuffer(
                    _device, VertexPositionColorTexture.VertexDeclaration, minVertices, BufferUsage.WriteOnly);
            }

            int minIndices = itemCount * 6; // 6 indices per item
            if (_indexBuffer == null || minIndices > _indexBuffer.Capacity)
            {
                _indexBuffer?.Dispose();
                _indexBuffer = new IndexBuffer(_device, IndexElementType.Int16, minIndices, BufferUsage.WriteOnly);

                // Use the quad buffer as a temporary buffer as it is always larger than the index buffer.
                Span<ushort> indexSpan = MemoryMarshal.Cast<SpriteQuad, ushort>(_quadBuffer).Slice(0, minIndices);

                for (int i = 0, v = 0; i < indexSpan.Length; i += 6, v += 4)
                {
                    /*
                     *  TL    TR    0,1,2,3 = index offsets for vertex indices
                     *   0----1     TL,TR,BL,BR are vertex references in SpriteBatchItem.   
                     *   |   /|    
                     *   |  / |
                     *   | /  |
                     *   |/   |
                     *   2----3
                     *  BL    BR
                     */

                    // Triangle 1
                    indexSpan[i + 0] = (ushort) (v + 0);
                    indexSpan[i + 1] = (ushort) (v + 1);
                    indexSpan[i + 2] = (ushort) (v + 2);

                    // Triangle 2
                    indexSpan[i + 3] = (ushort) (v + 1);
                    indexSpan[i + 4] = (ushort) (v + 3);
                    indexSpan[i + 5] = (ushort) (v + 2);
                }

                _indexBuffer.SetData(indexSpan);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpriteBatchItem GetBatchItem()
        {
            SpriteBatchItem[] items = _batchItems;
            int size = _itemCount;
            if ((uint) size < (uint) items.Length)
            {
                _itemCount = size + 1;
                return items[size];
            }
            return GrowAndGetBatchItem();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private SpriteBatchItem GrowAndGetBatchItem()
        {
            int oldSize = _batchItems.Length;
            int newSize = oldSize * 2;

            Array.Resize(ref _batchItems, newSize);
            for (int i = oldSize; i < newSize; i++)
                _batchItems[i] = new SpriteBatchItem();

            SetupBuffers(Math.Min(newSize, MaxBatchSize));
            return _batchItems[_itemCount++];
        }

        /// <summary>
        /// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not
        /// overflow the 16-bit array indices for vertices.
        /// </summary>
        /// <param name="sortMode">The type of depth sorting desired for the rendering.</param>
        /// <param name="effect">The custom effect to apply to the drawn geometry</param>
        public unsafe void DrawBatch(SpriteSortMode sortMode, Effect? effect)
        {
            if (effect != null && effect.IsDisposed)
            {
                ThrowHelper.Argument("The effect is disposed.", nameof(effect));
            }

            // nothing to do
            Span<SpriteBatchItem> items = _batchItems.AsSpan(0, _itemCount);
            if (items.Length == 0)
                return;

            // sort the batch items
            switch (sortMode)
            {
                case SpriteSortMode.Texture:
                case SpriteSortMode.FrontToBack:
                case SpriteSortMode.BackToFront:
                    items.Sort();
                    break;
            }

            // iterate through the batches, doing clamped sets of vertices at the time
            Span<SpriteQuad> quadBuffer = _quadBuffer.AsSpan();

            while (items.Length > 0)
            {
                Span<SpriteBatchItem> batchItems = items.Slice(0, Math.Min(items.Length, MaxBatchSize));

                int count = 0;
                Texture2D? tex = null;

                // TODO: create a path with sort-by-texture that does less ref-equality checks
                // (having such a path allows copying multiple items at once)

                // draw the batches
                for (int i = 0; i < batchItems.Length; i++, count++)
                {
                    SpriteBatchItem item = batchItems[i];

                    // if the texture changed, we need to flush and bind the new texture
                    if (!ReferenceEquals(item.Texture, tex))
                    {
                        Debug.Assert(item.Texture != null);

                        FlushQuads(quadBuffer.Slice(0, count), effect, tex);
                        count = 0;

                        tex = item.Texture;
                        _device.Textures[0] = tex;
                    }
                    item.Texture = null; // release texture from item

                    quadBuffer[count] = item.Quad;
                }

                // flush the remaining data
                FlushQuads(quadBuffer.Slice(0, count), effect, tex);

                items = items.Slice(batchItems.Length);
            }

            unchecked
            {
                _device._graphicsMetrics._spriteCount += _itemCount;
            }
            _itemCount = 0;
        }

        /// <summary>
        /// Sends the triangle list to the graphics device. Here is where the actual drawing starts.
        /// </summary>
        /// <param name="quads">The vertices to draw.</param>
        /// <param name="effect">The custom effect to apply to the geometry.</param>
        /// <param name="texture">The texture to draw.</param>
        private void FlushQuads(
            ReadOnlySpan<SpriteQuad> quads, Effect? effect, Texture texture)
        {
            if (quads.IsEmpty)
                return;

            Debug.Assert(_indexBuffer != null);
            Debug.Assert(_vertexBuffer != null);
            const PrimitiveType primitiveType = PrimitiveType.TriangleList;

            var vertices = MemoryMarshal.Cast<SpriteQuad, VertexPositionColorTexture>(quads);
            _vertexBuffer.SetData(vertices);

            _device.SetVertexBuffer(_vertexBuffer);
            _device.Indices = _indexBuffer;

            if (effect == null)
            {
                // If no custom effect is defined, then simply render.
                _device.DrawIndexedPrimitives(primitiveType, 0, 0, vertices.Length / 2);
            }
            else
            {
                // If the effect is not null, then apply each pass and render the geometry
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    // Whatever happens in pass.Apply, make sure the texture being drawn
                    // ends up in Textures[0].
                    _device.Textures[0] = texture;

                    _device.DrawIndexedPrimitives(primitiveType, 0, 0, vertices.Length / 2);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                _indexBuffer?.Dispose();
                _vertexBuffer?.Dispose();
                IsDisposed = true;
            }
        }

        ~SpriteBatcher()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

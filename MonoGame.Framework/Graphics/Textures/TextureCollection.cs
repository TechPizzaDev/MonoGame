// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Graphics
{
    public sealed partial class TextureCollection
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture?[] _textures;
        private readonly bool _applyToVertexStage;
        private int _dirty;

        internal TextureCollection(GraphicsDevice graphicsDevice, int maxTextures, bool applyToVertexStage)
        {
            _graphicsDevice = graphicsDevice;
            _textures = new Texture[maxTextures];
            _applyToVertexStage = applyToVertexStage;
            _dirty = int.MaxValue;
            PlatformInit();
        }

        public Texture? this[int index]
        {
            get => _textures[index];
            set
            {
                if (_applyToVertexStage && !_graphicsDevice.Capabilities.SupportsVertexTextures)
                    ThrowHelper.NotSupported("Vertex textures are not supported on this device.");

                var textures = _textures;
                if (textures[index] == value)
                    return;

                textures[index] = value;
                _dirty |= 1 << index;
            }
        }

        public ReadOnlyMemory<Texture?> GetTextures()
        {
            return _textures.AsMemory();
        }

        public ReadOnlySpan<Texture?> GetTexturesSpan()
        {
            return _textures.AsSpan();
        }

        internal void Clear()
        {
            _textures.AsSpan().Clear();

            PlatformClear();
            _dirty = int.MaxValue;
        }

        /// <summary>
        /// Marks all texture slots as dirty.
        /// </summary>
        internal void MarkDirty()
        {
            _dirty = int.MaxValue;
        }

        internal void SetTextures(GraphicsDevice device)
        {
            if (_applyToVertexStage && !device.Capabilities.SupportsVertexTextures)
                return;
            PlatformSetTextures(device);
        }
    }
}

// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.Direct3D11;

namespace MonoGame.Framework.Graphics
{
    public sealed partial class TextureCollection
    {
        void PlatformInit()
        {
        }

        internal void ClearTargets(GraphicsDevice device, RenderTargetBinding[] targets)
        {
            if (_applyToVertexStage && !device.Capabilities.SupportsVertexTextures)
                return;

            DeviceContext ctx = device._d3dContext;
            CommonShaderStage shader = _applyToVertexStage ? ctx.VertexShader : ctx.PixelShader;
            ClearTargets(targets, shader);
        }

        private void ClearTargets(RenderTargetBinding[] targets, CommonShaderStage shaderStage)
        {
            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // We assume 4 targets to avoid a loop within a loop below.
            var target0 = targets[0].RenderTarget;
            var target1 = targets[1].RenderTarget;
            var target2 = targets[2].RenderTarget;
            var target3 = targets[3].RenderTarget;

            // Make one pass across all the texture slots.
            var textures = _textures;
            for (var i = 0; i < textures.Length; i++)
            {
                var texture = textures[i];
                if (texture == null)
                    continue;

                if (texture != target0 &&
                    texture != target1 &&
                    texture != target2 &&
                    texture != target3)
                    continue;

                // Immediately clear the texture from the device.
                _dirty &= ~(1 << i);
                textures[i] = null;
                shaderStage.SetShaderResource(i, null);
            }
        }

        void PlatformClear()
        {
        }

        void PlatformSetTextures(GraphicsDevice device)
        {
            // Skip out if nothing has changed.
            if (_dirty == 0)
                return;

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.
            DeviceContext ctx = device._d3dContext;
            CommonShaderStage shaderStage = _applyToVertexStage ? ctx.VertexShader : ctx.PixelShader;

            var textures = _textures;
            for (var i = 0; i < textures.Length; i++)
            {
                var mask = 1 << i;
                if ((_dirty & mask) == 0)
                    continue;

                var tex = textures[i];
                if (tex == null || tex.IsDisposed)
                    shaderStage.SetShaderResource(i, null);
                else
                {
                    shaderStage.SetShaderResource(i, tex.GetShaderResourceView());
                    unchecked
                    {
                        _graphicsDevice._graphicsMetrics._textureCount++;
                    }
                }
                _dirty &= ~mask;
                if (_dirty == 0)
                    break;
            }

            _dirty = 0;
        }
    }
}

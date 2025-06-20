// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonoGame.Framework.Graphics
{
    /// <summary>
    /// Helper class for drawing text and sprites in optimized batches.
    /// </summary>
    public class SpriteBatch : GraphicsResource, ISpriteBatch
    {
        private SpriteBatcher _batcher;
        private EffectPass _spritePass;

        private bool _beginCalled;
        private SpriteSortMode _sortMode;
        private Effect? _effect;

#pragma warning disable CA2213 // Disposable fields should be disposed
        private BlendState _blendState = BlendState.AlphaBlend;
        private SamplerState _samplerState = SamplerState.LinearClamp;
        private DepthStencilState _depthStencilState = DepthStencilState.None;
        private RasterizerState _rasterizerState = RasterizerState.CullCounterClockwise;
#pragma warning restore CA2213

        /// <summary>
        /// Gets the default effect used by this <see cref="SpriteBatch"/>.
        /// </summary>
        public SpriteEffect SpriteEffect { get; }

        #region Constructors

        /// <summary>
        /// Constructs the <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/> which will be used for sprite rendering.</param>
        /// <exception cref="ArgumentNullException"><paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            _batcher = new SpriteBatcher(GraphicsDevice);
            SpriteEffect = new SpriteEffect(GraphicsDevice);
            _spritePass = SpriteEffect.CurrentTechnique.Passes[0];

            _beginCalled = false;
        }

        #endregion

        #region Begin

        /// <summary>
        /// Begins a new sprite and text batch with the specified render state.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="SpriteSortMode.Deferred"/> by default.</param>
        /// <param name="blendState">State of the blending. Uses <see cref="BlendState.AlphaBlend"/> if null.</param>
        /// <param name="samplerState">State of the sampler. Uses <see cref="SamplerState.LinearClamp"/> if null.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="DepthStencilState.None"/> if null.</param>
        /// <param name="rasterizerState">State of the rasterization. Uses <see cref="RasterizerState.CullCounterClockwise"/> if null.</param>
        /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect. Uses default sprite effect if null.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix4x4.Identity"/> if null.</param>
        /// <exception cref="InvalidOperationException"><see cref="Begin"/> is called next time without previous <see cref="End"/>.</exception>
        /// <remarks>
        /// <see cref="Begin"/> should be called before draw methods,
        /// and you cannot call it again before subsequent <see cref="End"/>.
        /// </remarks>
        public void Begin(
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState? blendState = null,
            SamplerState? samplerState = null,
            DepthStencilState? depthStencilState = null,
            RasterizerState? rasterizerState = null,
            Effect? effect = null,
            Matrix4x4? transformMatrix = null)
        {
            if (_beginCalled)
                ThrowHelper.InvalidOperation("Begin cannot be called again until End has been called.");

            // defaults
            _sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;
            SpriteEffect.TransformMatrix = transformMatrix;

            // Setup things now so a user can change them.
            if (sortMode == SpriteSortMode.Immediate)
                Setup();

            _beginCalled = true;
        }

        #endregion

        #region End, Flush

        /// <summary>
        /// Flushes all batched sprites to the graphics device 
        /// without ending the current state.
        /// </summary>
        public void Flush()
        {
            AssertBeginCalled();

            _batcher.DrawBatch(_sortMode, _effect);
        }

        /// <summary>
        /// Flushes if the sort mode is <see cref="SpriteSortMode.Immediate"/>.
        /// </summary>
        public bool FlushIfNeeded()
        {
            if (_sortMode == SpriteSortMode.Immediate)
            {
                Flush();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Flushes all batched text and sprites to the screen
        /// ending the current state.
        /// </summary>
        public void End()
        {
            AssertBeginCalled();

            _beginCalled = false;

            if (_sortMode != SpriteSortMode.Immediate)
                Setup();

            _batcher.DrawBatch(_sortMode, _effect);
        }

        #endregion

        #region Helpers

        /// <inheritdoc/>
        public ref SpriteQuad GetBatchQuad(Texture2D texture, float sortKey)
        {
            var item = _batcher.GetBatchItem();
            item.Texture = texture;
            item.SortKey = sortKey;
            return ref item.Quad;
        }

        /// <inheritdoc/>
        public float GetSortKey(Texture2D texture, float sortDepth)
        {
            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    return texture.SortingKey;

                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    return sortDepth;

                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    return -sortDepth;

                default:
                    return sortDepth;
            }
        }

        private void Setup()
        {
            var gd = GraphicsDevice;
            gd.BlendState = _blendState;
            gd.DepthStencilState = _depthStencilState;
            gd.RasterizerState = _rasterizerState;
            gd.SamplerStates[0] = _samplerState;

            _spritePass.Apply();
        }

        private void AssertBeginCalled([CallerMemberName] string? callerName = null)
        {
            if (!_beginCalled)
            {
                Throw(callerName);
            }

            static void Throw(string? callerName)
            {
                ThrowHelper.InvalidOperation($"{nameof(Begin)} must be called before you can call {callerName}.");
            }
        }

        #endregion

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen or null if <paramref name="destinationRectangle"> is used.</paramref></param>
        /// <param name="destinationRectangle">The drawing bounds on screen or null if <paramref name="position"> is used.</paramref></param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="origin">An optional center of rotation. Uses <see cref="Vector2.Zero"/> if null.</param>
        /// <param name="rotation">An optional rotation of this sprite. 0 by default.</param>
        /// <param name="scale">An optional scale vector. Uses <see cref="Vector2.One"/> if null.</param>
        /// <param name="color">An optional color mask. Uses <see cref="Color.White"/> if null.</param>
        /// <param name="flip">Optional sprite mirroring. <see cref="SpriteFlip.None"/> by default.</param>
        /// <param name="layerDepth">An optional depth of the layer of this sprite. 0 by default.</param>
        /// <exception cref="ArgumentException">
        /// Throwns if both <paramref name="position"/> and <paramref name="destinationRectangle"/> been used.
        /// </exception>
        /// <remarks>
        /// This overload uses optional parameters. 
        /// This overload requires only one of <paramref name="position"/> and 
        /// <paramref name="destinationRectangle"/> been used.
        /// </remarks>
        [Obsolete("In future versions this method can be removed.")]
        public void Draw(
            Texture2D texture,
            Vector2? position = null,
            RectangleF? destinationRectangle = null,
            RectangleF? sourceRectangle = null,
            Vector2? origin = null,
            float rotation = 0f,
            Vector2? scale = null,
            Color? color = null,
            SpriteFlip flip = SpriteFlip.None,
            float layerDepth = 0f)
        {
            // Assign default values to null parameters here, as they are not compile-time constants
            if (!color.HasValue)
                color = Color.White;

            if (!origin.HasValue)
                origin = Vector2.Zero;

            if (!scale.HasValue)
                scale = Vector2.One;

            // If both drawRectangle and position are null,
            // or if both have been assigned a value, raise an error
            if (destinationRectangle.HasValue == position.HasValue)
            {
                ThrowHelper.Argument("Expected drawRectangle or position, but received neither or both.");
            }

            if (position.HasValue) // Call Draw() using position
            {
                Draw(
                    texture,
                    position.GetValueOrDefault(),
                    sourceRectangle,
                    color.GetValueOrDefault(),
                    rotation,
                    origin.GetValueOrDefault(),
                    scale.GetValueOrDefault(),
                    flip,
                    layerDepth);
            }
            else // Call Draw() using drawRectangle
            {
                Draw(
                    texture,
                    destinationRectangle.GetValueOrDefault(),
                    sourceRectangle,
                    color.GetValueOrDefault(),
                    rotation,
                    origin.GetValueOrDefault(),
                    flip,
                    layerDepth);
            }
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">
        /// An optional region on the texture which will be rendered.
        /// If null; draws full texture.
        /// </param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="flip">Sprite mirroring flags. Can be combined.</param>
        /// <param name="sortDepth">A depth of the layer of this sprite.</param>
        public void Draw(
            Texture2D texture, Vector2 position, RectangleF? sourceRectangle, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteFlip flip, float sortDepth)
        {
            ArgumentNullException.ThrowIfNull(texture);
            AssertBeginCalled();

            origin *= scale;

            Vector4 texCoord;
            float w, h;
            if (sourceRectangle.HasValue)
            {
                RectangleF srcRect = sourceRectangle.GetValueOrDefault();
                w = srcRect.Width * scale.X;
                h = srcRect.Height * scale.Y;
                texCoord = SpriteQuad.GetTexCoord(texture.TexelSize, srcRect);
            }
            else
            {
                w = texture.Width * scale.X;
                h = texture.Height * scale.Y;
                texCoord = new Vector4(0, 0, 1, 1);
            }
            SpriteQuad.FlipTexCoords(ref texCoord, flip);

            float sortKey = GetSortKey(texture, sortDepth);
            ref var quad = ref GetBatchQuad(texture, sortKey);
            if (rotation == 0f)
            {
                quad.Set(
                    position.X - origin.X, position.Y - origin.Y, w, h,
                    color, texCoord, sortDepth);
            }
            else
            {
                quad.Set(
                    position.X, position.Y, -origin.X, -origin.Y, w, h,
                    MathF.Sin(rotation), MathF.Cos(rotation),
                    color, texCoord, sortDepth);
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">
        /// An optional region on the texture which will be rendered, drawing full texture otherwise.
        /// </param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="flip">Sprite mirroring flags. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        public void Draw(
            Texture2D texture,
            Vector2 position,
            RectangleF? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            float scale,
            SpriteFlip flip,
            float layerDepth)
        {
            Draw(
                texture, position, sourceRectangle, color,
                rotation, origin, new Vector2(scale, scale), flip, layerDepth);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">
        /// An optional region on the texture which will be rendered, drawing full texture otherwise.
        /// </param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="flip">Sprite mirroring flags. Can be combined.</param>
        /// <param name="sortDepth">A depth of the layer of this sprite.</param>
        public void Draw(
            Texture2D texture,
            RectangleF destinationRectangle,
            RectangleF? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            SpriteFlip flip,
            float sortDepth)
        {
            AssertValidArguments(texture, nameof(Draw));

            var texelSize = texture.TexelSize;
            Vector4 texCoord;
            if (sourceRectangle.HasValue)
            {
                RectangleF srcRect = sourceRectangle.GetValueOrDefault();
                texCoord = SpriteQuad.GetTexCoord(texelSize, srcRect);
                origin = SpriteQuad.RemapOrigin(origin, texelSize, destinationRectangle, srcRect);
            }
            else
            {
                texCoord = new Vector4(0, 0, 1, 1);
                origin = SpriteQuad.RemapOrigin(origin, texelSize, destinationRectangle);
            }
            SpriteQuad.FlipTexCoords(ref texCoord, flip);

            float sortKey = GetSortKey(texture, sortDepth);
            ref var quad = ref GetBatchQuad(texture, sortKey);
            if (rotation == 0f)
            {
                quad.Set(
                    destinationRectangle.X - origin.X, destinationRectangle.Y - origin.Y,
                    destinationRectangle.Width, destinationRectangle.Height,
                    color, texCoord, sortDepth);
            }
            else
            {
                quad.Set(
                    destinationRectangle.X, destinationRectangle.Y, -origin.X, -origin.Y,
                    destinationRectangle.Width, destinationRectangle.Height,
                    MathF.Sin(rotation), MathF.Cos(rotation),
                    color, texCoord, sortDepth);
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, Vector2 position, RectangleF? sourceRectangle, Color color)
        {
            ArgumentNullException.ThrowIfNull(texture);
            AssertBeginCalled();

            Vector4 texCoord;
            float w;
            float h;
            if (sourceRectangle.HasValue)
            {
                RectangleF srcRect = sourceRectangle.GetValueOrDefault();
                var texelSize = texture.TexelSize;

                w = srcRect.Width;
                h = srcRect.Height;
                texCoord = SpriteQuad.GetTexCoord(texelSize, srcRect);
            }
            else
            {
                w = texture.Width;
                h = texture.Height;
                texCoord = new Vector4(0, 0, 1, 1);
            }

            float sortKey = GetSortKey(texture, 0);
            GetBatchQuad(texture, sortKey).Set(position.X, position.Y, w, h, color, texCoord, 0);
            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">
        /// An optional region on the texture which will be rendered. If null - draws full texture.
        /// </param>
        /// <param name="color">A color mask.</param>
        public void Draw(
            Texture2D texture, RectangleF destinationRectangle, RectangleF? sourceRectangle, Color color)
        {
            ArgumentNullException.ThrowIfNull(texture);
            AssertBeginCalled();

            Vector4 texCoord;
            if (sourceRectangle.HasValue)
            {
                RectangleF srcRect = sourceRectangle.GetValueOrDefault();
                texCoord = SpriteQuad.GetTexCoord(texture.TexelSize, srcRect);
            }
            else
            {
                texCoord = new Vector4(0, 0, 1, 1);
            }

            float sortKey = GetSortKey(texture, 0);
            GetBatchQuad(texture, sortKey).Set(
                destinationRectangle.X, destinationRectangle.Y,
                destinationRectangle.Width, destinationRectangle.Height,
                color, texCoord, 0);

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, Vector2 position, Color color)
        {
            ArgumentNullException.ThrowIfNull(texture);
            AssertBeginCalled();

            float sortKey = GetSortKey(texture, 0);
            GetBatchQuad(texture, sortKey).Set(
                position.X, position.Y,
                texture.Width, texture.Height,
                color, new Vector4(0, 0, 1, 1), 0);

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, RectangleF destinationRectangle, Color color)
        {
            ArgumentNullException.ThrowIfNull(texture);
            AssertBeginCalled();

            float sortKey = GetSortKey(texture, 0);
            GetBatchQuad(texture, sortKey).Set(
                destinationRectangle.X, destinationRectangle.Y,
                destinationRectangle.Width, destinationRectangle.Height,
                color, new Vector4(0, 0, 1, 1), 0);

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit text for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        public void DrawString(
            SpriteFont spriteFont, RuneEnumerator text, Vector2 position, Color color)
        {
            ArgumentNullException.ThrowIfNull(spriteFont);
            AssertBeginCalled();

            float sortKey = GetSortKey(spriteFont.Texture, 0);

            var offset = Vector2.Zero;
            var texelSize = spriteFont.Texture.TexelSize;
            bool firstGlyphOfLine = true;
            var glyphs = spriteFont.GetGlyphSpan();

            foreach (Rune c in text)
            {
                if (c == (Rune)'\r')
                    continue;

                if (c == (Rune)'\n')
                {
                    offset.X = 0;
                    offset.Y += spriteFont.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                int glyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                ref readonly SpriteFont.Glyph glyph = ref glyphs[glyphIndex];

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                // so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(glyph.LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += spriteFont.Spacing + glyph.LeftSideBearing;
                }

                var p = offset;
                p.X += glyph.Cropping.X;
                p.Y += glyph.Cropping.Y;
                p += position;

                var texCoord = SpriteQuad.GetTexCoord(texelSize, glyph.BoundsInTexture);

                GetBatchQuad(spriteFont.Texture, sortKey).Set(
                    p.X, p.Y, glyph.BoundsInTexture.Width, glyph.BoundsInTexture.Height,
                    color, texCoord, 0);

                offset.X += glyph.Width + glyph.RightSideBearing;
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit text for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="flip">Sprite mirroring flags. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public void DrawString(
            SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteFlip flip, float layerDepth)
        {
            DrawString(spriteFont, text, position, color, rotation, origin, new Vector2(scale), flip, layerDepth);
        }

        /// <summary>
        /// Submit text for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="flip">Sprite mirroring flags. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public void DrawString(
            SpriteFont spriteFont, RuneEnumerator text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteFlip flip, float layerDepth)
        {
            ArgumentNullException.ThrowIfNull(spriteFont);
            AssertBeginCalled();

            float sortKey = GetSortKey(spriteFont.Texture, layerDepth);

            var flipAdjustment = Vector2.Zero;
            bool flippedVert = (flip & SpriteFlip.Vertical) == SpriteFlip.Vertical;
            bool flippedHorz = (flip & SpriteFlip.Horizontal) == SpriteFlip.Horizontal;

            if (flippedVert || flippedHorz)
            {
                Vector2 size = spriteFont.MeasureString(text);
                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }
                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = spriteFont.LineSpacing - size.Y;
                }
            }

            var transformation = Matrix4x4.Identity;
            float cos = 0, sin = 0;
            if (rotation == 0)
            {
                transformation.M11 = flippedHorz ? -scale.X : scale.X;
                transformation.M22 = flippedVert ? -scale.Y : scale.Y;
                transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
                transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            }
            else
            {
                cos = MathF.Cos(rotation);
                sin = MathF.Sin(rotation);
                transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
                transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
                transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * -sin;
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;

                transformation.M41 =
                    ((flipAdjustment.X - origin.X) * transformation.M11) +
                    (flipAdjustment.Y - origin.Y) * transformation.M21 +
                    position.X;

                transformation.M42 =
                    ((flipAdjustment.X - origin.X) * transformation.M12) +
                    (flipAdjustment.Y - origin.Y) * transformation.M22 +
                    position.Y;
            }

            var offset = Vector2.Zero;
            var texelSize = spriteFont.Texture.TexelSize;
            var firstGlyphOfLine = true;
            var glyphs = spriteFont.GetGlyphSpan();

            foreach (Rune c in text)
            {
                if (c == (Rune)'\r')
                    continue;

                if (c == (Rune)'\n')
                {
                    offset.X = 0;
                    offset.Y += spriteFont.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                int glyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                ref readonly SpriteFont.Glyph glyph = ref glyphs[glyphIndex];

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(glyph.LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += spriteFont.Spacing + glyph.LeftSideBearing;
                }

                Vector2 pos = offset;

                if (flippedHorz)
                    pos.X += glyph.BoundsInTexture.Width;
                pos.X += glyph.Cropping.X;

                if (flippedVert)
                    pos.Y += glyph.BoundsInTexture.Height - spriteFont.LineSpacing;
                pos.Y += glyph.Cropping.Y;

                pos = Vector2.Transform(pos, transformation);

                var texCoord = SpriteQuad.GetTexCoord(texelSize, glyph.BoundsInTexture);
                SpriteQuad.FlipTexCoords(ref texCoord, flip);

                ref var quad = ref GetBatchQuad(spriteFont.Texture, sortKey);
                if (rotation == 0f)
                {
                    quad.Set(
                        pos.X, pos.Y,
                        glyph.BoundsInTexture.Width * scale.X,
                        glyph.BoundsInTexture.Height * scale.Y,
                        color,
                        texCoord,
                        layerDepth);
                }
                else
                {
                    quad.Set(
                        pos.X, pos.Y, 0, 0,
                        glyph.BoundsInTexture.Width * scale.X,
                        glyph.BoundsInTexture.Height * scale.Y,
                        sin, cos,
                        color,
                        texCoord,
                        layerDepth);
                }

                offset.X += glyph.Width + glyph.RightSideBearing;
            }

            FlushIfNeeded();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    SpriteEffect?.Dispose();
                    _batcher?.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
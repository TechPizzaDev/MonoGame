// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MonoGame.Framework.Graphics
{
    public partial class SpriteFont : IEnumerable<KeyValuePair<Rune, int>>
    {
        private Dictionary<Rune, int> _glyphIndexMap;
        private Glyph[] _glyphs;
        private Rune? _defaultCharacter;
        private int _defaultGlyphIndex = -1;

        /// <summary>
        /// Gets a collection of the characters in the font.
        /// </summary>
        public Dictionary<Rune, int>.KeyCollection Characters => _glyphIndexMap.Keys;

        /// <summary>
        /// Gets a collection of the glyph indicies in the font.
        /// </summary>
        public Dictionary<Rune, int>.ValueCollection GlyphIndices => _glyphIndexMap.Values;

        /// <summary>
        /// Gets the texture that the font draws from.
        /// </summary>
        /// <remarks>
        /// Can be used to implement custom rendering of a <see cref="SpriteFont"/>.
        /// </remarks>
        public Texture2D Texture { get; }

        /// <summary>
        /// Gets or sets the line spacing (the distance from baseline
        /// to baseline) of the font.
        /// </summary>
        public int LineSpacing { get; set; }

        /// <summary>
        /// Gets or sets the spacing (tracking) between characters in
        /// the font.
        /// </summary>
        public float Spacing { get; set; }

        /// <summary>
        /// Gets or sets the character that will be substituted when a
        /// given character is not included in the font.
        /// </summary>
        public Rune? DefaultCharacter
        {
            get => _defaultCharacter;
            set
            {
                // Get the default glyph index here once.
                if (value.HasValue)
                {
                    if (!TryGetGlyphIndex(value.Value, out _defaultGlyphIndex))
                        throw new ArgumentException("Character cannot be resolved by this font.");
                }
                else
                {
                    _defaultGlyphIndex = -1;
                }
                _defaultCharacter = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteFont" /> class.
        /// </summary>
        /// <param name="texture">The font texture.</param>
        /// <param name="glyphBounds">The rectangles in the font texture containing letters.</param>
        /// <param name="cropping">
        /// The cropping rectangles, which are applied to the corresponding glyphBounds
        /// to calculate the bounds of the actual character.
        /// </param>
        /// <param name="characters">The characters.</param>
        /// <param name="lineSpacing">
        /// The line spacing (the distance from baseline to baseline) of the font.
        /// </param>
        /// <param name="spacing">
        /// The spacing (tracking) between characters in the font.
        /// </param>
        /// <param name="kerning">
        /// The letters kernings(X - left side bearing, Y - width and Z - right side bearing).
        /// </param>
        /// <param name="defaultCharacter">
        /// The character that will be substituted when a given character is not included in the font.
        /// </param>
        public SpriteFont(
            Texture2D texture,
            IReadOnlyList<Rectangle> glyphBounds,
            IReadOnlyList<Rectangle> cropping,
            IReadOnlyList<Rune> characters,
            int lineSpacing,
            float spacing,
            IReadOnlyList<Vector3> kerning,
            Rune? defaultCharacter)
        {
            // TODO: better argument validation
            if (glyphBounds == null) throw new ArgumentNullException(nameof(glyphBounds));
            if (cropping == null) throw new ArgumentNullException(nameof(cropping));
            if (characters == null) throw new ArgumentNullException(nameof(characters));
            if (kerning == null) throw new ArgumentNullException(nameof(kerning));

            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            LineSpacing = lineSpacing;
            Spacing = spacing;
            DefaultCharacter = defaultCharacter;

            _glyphIndexMap = new Dictionary<Rune, int>(characters.Count);
            _glyphs = new Glyph[characters.Count];
            for (int i = 0; i < _glyphs.Length; i++)
            {
                _glyphs[i] = new Glyph(
                    boundsInTexture: glyphBounds[i],
                    cropping: cropping[i],
                    character: characters[i],

                    leftSideBearing: kerning[i].X,
                    rightSideBearing: kerning[i].Z,
                    width: kerning[i].Y);

                _glyphIndexMap.Add(_glyphs[i].Character, i);
            }
        }

        public ReadOnlyMemory<Glyph> GetGlyphs()
        {
            return _glyphs.AsMemory();
        }

        public ReadOnlySpan<Glyph> GetGlyphSpan()
        {
            return _glyphs.AsSpan();
        }

        /// <summary>
        /// Returns the size of text rendered in the font.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <returns>The size, in pixels, of <paramref name="text"/> when rendered in the font.</returns>
        public SizeF MeasureString(RuneEnumerator text)
        {
            float width = 0f;
            float finalLineHeight = LineSpacing;
            bool firstGlyphOfLine = true;
            var offset = Vector2.Zero;

            foreach (var c in text)
            {
                if (c.Value == '\r')
                    continue;

                if (c.Value == '\n')
                {
                    finalLineHeight = LineSpacing;

                    offset.X = 0;
                    offset.Y += LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                int glyphIndex = GetGlyphIndexOrDefault(c);
                ref readonly Glyph glyph = ref _glyphs[glyphIndex];

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
                    offset.X += Spacing + glyph.LeftSideBearing;
                }

                offset.X += glyph.Width;

                float proposedWidth = offset.X + Math.Max(glyph.RightSideBearing, 0);
                if (proposedWidth > width)
                    width = proposedWidth;

                offset.X += glyph.RightSideBearing;

                if (glyph.Cropping.Height > finalLineHeight)
                    finalLineHeight = glyph.Cropping.Height;
            }

            return new SizeF(width, offset.Y + finalLineHeight);
        }

        public bool TryGetGlyphIndex(Rune rune, out int index)
        {
            return _glyphIndexMap.TryGetValue(rune, out index);
        }

        public int GetGlyphIndexOrDefault(Rune rune)
        {
            if (_glyphIndexMap.TryGetValue(rune, out int index))
                return index;

            if (_defaultGlyphIndex == -1)
                Throw(rune);

            return _defaultGlyphIndex;

            static void Throw(Rune rune)
            {
                throw new KeyNotFoundException(
                    "Character cannot be resolved and no default glyph has been assigned to this font. Key: " + rune);
            }
        }

        public Dictionary<Rune, int>.Enumerator GetEnumerator()
        {
            return _glyphIndexMap.GetEnumerator();
        }

        IEnumerator<KeyValuePair<Rune, int>> IEnumerable<KeyValuePair<Rune, int>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

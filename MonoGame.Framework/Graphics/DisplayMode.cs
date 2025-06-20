// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

namespace MonoGame.Framework.Graphics
{
    [DataContract]
    public class DisplayMode : IEquatable<DisplayMode>
    {
        public SurfaceFormat Format { get; }
        public int Height { get; }
        public int Width { get; }

        public float AspectRatio => Width / (float)Height;
        public Rectangle TitleSafeArea => GraphicsDevice.GetTitleSafeArea(0, 0, Width, Height);

        internal DisplayMode(int width, int height, SurfaceFormat format)
        {
            Width = width;
            Height = height;
            Format = format;
        }

        #region Operators

        public static bool operator ==(DisplayMode left, DisplayMode right)
        {
            if (ReferenceEquals(left, right)) //Same object or both are null
                return true;

            if (left is null || right is null)
                return false;

            return (left.Format == right.Format)
                && (left.Height == right.Height)
                && (left.Width == right.Width);
        }

        public static bool operator !=(DisplayMode left, DisplayMode right)
        {
            return !(left == right);
        }

        #endregion Operators

        #region Public Methods

        public bool Equals(DisplayMode? other)
        {
            return this == other;
        }

        public override bool Equals(object? obj)
        {
            return obj is DisplayMode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height, Format);
        }

        public override string ToString()
        {
            return "{Width:" + Width + " Height:" + Height + " Format:" + Format + " AspectRatio:" + AspectRatio + "}";
        }

        #endregion Public Methods
    }
}

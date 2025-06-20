// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;

namespace MonoGame.Framework.Graphics
{
    /// <summary>
    /// Describes the view bounds for render-target surface.
    /// </summary>
    public readonly struct Viewport : IEquatable<Viewport>
    {
        #region Properties

        /// <summary>
        /// The x coordinate of the beginning of this viewport.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The y coordinate of the beginning of this viewport.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// The width of the bounds in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the bounds in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The lower limit of depth of this viewport.
        /// </summary>
        public float MinDepth { get; }

        /// <summary>
        /// The upper limit of depth of this viewport.
        /// </summary>
        public float MaxDepth { get; }

        #endregion

        /// <summary>
        /// Gets the aspect ratio of this <see cref="Viewport"/>, which is width / height. 
        /// </summary>
        public readonly float AspectRatio
        {
            get
            {
                if (Height != 0 && Width != 0)
                    return Width / (float)Height;
                return 0f;
            }
        }

        /// <summary>
        /// Gets or sets a boundary of this <see cref="Viewport"/>.
        /// </summary>
        public Rectangle Bounds => new(X, Y, Width, Height);

        /// <summary>
        /// Returns the subset of the viewport that is guaranteed to be visible on a lower quality display.
        /// </summary>
        public Rectangle TitleSafeArea => GraphicsDevice.GetTitleSafeArea(X, Y, Width, Height);

        #region Constructors

        /// <summary>
        /// Constructs a viewport from the given values.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the view bounds in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the view bounds in pixels.</param>
        /// <param name="width">The width of the view bounds in pixels.</param>
        /// <param name="height">The height of the view bounds in pixels.</param>
        /// <param name="minDepth">The lower limit of depth.</param>
        /// <param name="maxDepth">The upper limit of depth.</param>
        public Viewport(int x, int y, int width, int height, float minDepth, float maxDepth)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Constructs a viewport from the given values. 
        /// The <see cref="MinDepth"/> will be zero and <see cref="MaxDepth"/> will be one.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the view bounds in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the view bounds in pixels.</param>
        /// <param name="width">The width of the view bounds in pixels.</param>
        /// <param name="height">The height of the view bounds in pixels.</param>
        public Viewport(int x, int y, int width, int height) :
            this(x, y, width, height, minDepth: 0, maxDepth: 1)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">
        /// A <see cref="Rectangle"/> that defines the location and size of
        /// the <see cref="Viewport"/> in a render target.
        /// </param>
        public Viewport(Rectangle bounds) : this(bounds.X, bounds.Y, bounds.Width, bounds.Height)
        {
        }

        #endregion

        #region Project

        /// <summary>
        /// Projects a <see cref="Vector3"/> from model space into screen space.
        /// The source point is transformed from model space to world space by the world matrix,
        /// then from world space to view space by the view matrix, and
        /// finally from view space to screen space by the projection matrix.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to project.</param>
        /// <param name="projection">The projection <see cref="Matrix4x4"/>.</param>
        /// <param name="view">The view <see cref="Matrix4x4"/>.</param>
        /// <param name="world">The world <see cref="Matrix4x4"/>.</param>
        /// <returns>The vector projected into screen space.</returns>
        public readonly Vector3 Project(
            Vector3 source, in Matrix4x4 projection, in Matrix4x4 view, in Matrix4x4 world)
        {
            var matrix = world * view * projection;
            var result = Vector3.Transform(source, matrix);

            float a =
                (source.X * matrix.M14) + 
                (source.Y * matrix.M24) + 
                (source.Z * matrix.M34) + 
                matrix.M44;

            if (!WithinEpsilon(a, 1f))
            {
                result.X /= a;
                result.Y /= a;
                result.Z /= a;
            }

            result.X = ((result.X + 1f) * 0.5f * Width) + X;
            result.Y = ((-result.Y + 1f) * 0.5f * Height) + Y;
            result.Z = (result.Z * (MaxDepth - MinDepth)) + MinDepth;

            return result;
        }

        #endregion

        #region Unproject

        /// <summary>
        /// Unprojects a <see cref="Vector3"/> from screen space into model space.
        /// The source point is transformed from screen space to 
        /// view space by the inverse of the projection matrix,
        /// then from view space to world space by the inverse of the view matrix, and
        /// finally from world space to model space by the inverse of the world matrix.
        /// Note source.Z must be less than or equal to MaxDepth.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to unproject.</param>
        /// <param name="projection">The projection <see cref="Matrix4x4"/>.</param>
        /// <param name="view">The view <see cref="Matrix4x4"/>.</param>
        /// <param name="world">The world <see cref="Matrix4x4"/>.</param>
        /// <returns>The vector vector unprojected into model space.</returns>
        public readonly Vector3 Unproject(
            Vector3 source, in Matrix4x4 projection, in Matrix4x4 view, in Matrix4x4 world)
        {
            var matrix = world * view * projection;
            if (!Matrix4x4.Invert(matrix, out matrix))
                throw new ArgumentException("Failed to invert final matrix.");

            var usource = new Vector3(
                ((source.X - X) / Width * 2f) - 1f,
                -(((source.Y - Y) / Height * 2f) - 1f),
                (source.Z - MinDepth) / (MaxDepth - MinDepth));

            var result = Vector3.Transform(usource, matrix);

            usource *= new Vector3(matrix.M14, matrix.M24, matrix.M34);
            float a = usource.X + usource.Y + usource.Z + matrix.M44;

            if (!WithinEpsilon(a, 1f))
                result /= a;

            return result;
        }

        #endregion

        private static bool WithinEpsilon(float a, float b)
        {
            float num = a - b;
            return (-1.401298E-45f <= num) && (num <= float.Epsilon);
        }

        public static bool operator ==(in Viewport a, in Viewport b)
        {
            return a.X == b.X
                && a.Y == b.Y
                && a.Width == b.Width
                && a.Height == b.Height
                && a.MinDepth == b.MinDepth
                && a.MaxDepth == b.MaxDepth;
        }

        public static bool operator !=(in Viewport a, in Viewport b)
        {
            return !(a == b);
        }

        public bool Equals(Viewport other) => this == other;

        public override bool Equals(object? obj) => obj is Viewport other && this == other;

        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height, MinDepth, MaxDepth);

        /// <summary>
        /// Returns a <see cref="string"/> representation of this <see cref="Viewport"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] 
        /// Width:[<see cref="Width"/>] Height:[<see cref="Height"/>] 
        /// MinDepth:[<see cref="MinDepth"/>] MaxDepth:[<see cref="MaxDepth"/>]}
        /// </summary>
        /// <returns>A <see cref="string"/> representation of this <see cref="Viewport"/>.</returns>
        public override string ToString()
        {
            return
                "{X:" + X + " Y:" + Y +
                " Width:" + Width + " Height:" + Height +
                " MinDepth:" + MinDepth + " MaxDepth:" + MaxDepth + "}";
        }
    }
}
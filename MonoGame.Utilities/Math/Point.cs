// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MonoGame.Framework
{
    /// <summary>
    /// Describes a 2D-point.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Point : IEquatable<Point>
    {
        /// <summary>
        /// <see cref="Point"/> with the coordinates (0, 0).
        /// </summary>
        public static Point Zero => default;

        #region Public Fields

        /// <summary>
        /// The x coordinate of this <see cref="Point"/>.
        /// </summary>
        [DataMember]
        public int X;

        /// <summary>
        /// The y coordinate of this <see cref="Point"/>.
        /// </summary>
        [DataMember]
        public int Y;

        #endregion

        #region Properties

        internal string DebuggerDisplay => string.Concat(
            X.ToString(), "  ",
            Y.ToString(), "  ");

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a point with X and Y from two values.
        /// </summary>
        /// <param name="x">The x coordinate in 2d-space.</param>
        /// <param name="y">The y coordinate in 2d-space.</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructs a point with X and Y set to the same value.
        /// </summary>
        /// <param name="value">The x and y coordinates in 2d-space.</param>
        public Point(int value)
        {
            X = value;
            Y = value;
        }

        #endregion

        public static Point Add(Point a, Point b) => a + b;

        public static Point Subtract(Point left, Point right) => left - right;

        public static Point Multiply(Point a, int b) => a * b;

        public static PointF Multiply(Point a, float b) => a * b;

        public static Point Divide(Point left, int divisor) => left / divisor;

        public static PointF Divide(Point left, float divisor) => left / divisor;

        #region Operators

        /// <summary>
        /// Adds two points.
        /// </summary>
        /// <param name="a">Source <see cref="Point"/> on the left of the add sign.</param>
        /// <param name="b">Source <see cref="Point"/> on the right of the add sign.</param>
        /// <returns>Sum of the points.</returns>
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// Subtracts a <see cref="Point"/> from a <see cref="Point"/>.
        /// </summary>
        /// <param name="left">Source <see cref="Point"/> on the left of the sub sign.</param>
        /// <param name="right">Source <see cref="Point"/> on the right of the sub sign.</param>
        /// <returns>Result of the subtraction.</returns>
        public static Point operator -(Point left, Point right)
        {
            return new Point(left.X - right.X, left.Y - right.Y);
        }

        public static Point operator *(Point a, int b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        public static PointF operator *(Point a, float b)
        {
            return (PointF)a * b;
        }

        public static Point operator /(Point left, int divisor)
        {
            return new Point(left.X / divisor, left.Y / divisor);
        }

        public static PointF operator /(Point left, float divisor)
        {
            return (PointF)left / divisor;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets a <see cref="Vector2"/> representation of this object.
        /// </summary>
        public readonly Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        /// <summary>
        /// Gets a <see cref="PointF"/> representation of this object.
        /// </summary>
        public readonly PointF ToPointF()
        {
            return new PointF(X, Y);
        }

        /// <summary>
        /// Gets a <see cref="SizeF"/> representation of this object.
        /// </summary>
        public readonly SizeF ToSizeF()
        {
            return new SizeF(X, Y);
        }

        /// <summary>
        /// Gets a <see cref="Size"/> representation of this object.
        /// </summary>
        public readonly Size ToSize()
        {
            return Unsafe.BitCast<Point, Size>(this);
        }

        /// <summary>
        /// Deconstruction method for <see cref="Point"/>.
        /// </summary>
        public readonly void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        #endregion

        #region Equals (operator ==, !=)

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Point"/>.
        /// </summary>
        /// <param name="other">The <see cref="Point"/> to compare.</param>
        public readonly bool Equals(Point other)
        {
            return this == other;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare.</param>
        public override readonly bool Equals(object? obj)
        {
            return obj is Point other && Equals(other);
        }

        /// <summary>
        /// Compares whether two <see cref="Point"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Point"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Point"/> instance on the right of the equal sign.</param>
        /// <returns><see langword="true"/> if the instances are equal; <see langword="false"/> otherwise.</returns>
        public static bool operator ==(Point a, Point b)
        {
            return (a.X == b.X) && (a.Y == b.Y);
        }

        /// <summary>
        /// Compares whether two <see cref="Point"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Point"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Point"/> instance on the right of the not equal sign.</param>
        /// <returns><see langword="true"/> if the instances are not equal; <see langword="false"/> otherwise.</returns>	
        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a hash code of this <see cref="Point"/>.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// Returns a <see cref="string"/> representation of this <see cref="Point"/>.
        /// </summary>
        public override string ToString() => $"{{X:{X}, Y:{Y}}}";

        #endregion

        #region Explicit operators

        public static explicit operator Point(Vector2 vector)
        {
            return vector.ToPoint();
        }

        public static explicit operator Point(PointF point)
        {
            return point.ToPoint();
        }

        public static explicit operator Point(SizeF size)
        {
            return size.ToPoint();
        }

        #endregion

        #region Implicit Operators

        public static implicit operator Vector2(Point point)
        {
            return point.ToVector2();
        }

        public static implicit operator PointF(Point point)
        {
            return point.ToPointF();
        }

        public static implicit operator SizeF(Point point)
        {
            return point.ToSizeF();
        }

        public static implicit operator Point(Size size)
        {
            return size.ToPoint();
        }

        #endregion
    }
}



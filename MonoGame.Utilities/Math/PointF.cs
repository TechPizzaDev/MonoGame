using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MonoGame.Framework
{
    // Real-Time Collision Detection, Christer Ericson, 2005. Chapter 3.2;
    // A Math and Geometry Primer - Coordinate Systems and Points. pg 35

    /// <summary>
    /// Describes a 2D-point.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   A point is a position in two-dimensional space, the location of which is described in terms of a
    ///   two-dimensional coordinate system, given by a reference point, called the origin, and two coordinate axes.
    ///   </para>
    ///   <para>
    ///   A common two-dimensional coordinate system is the Cartesian (or rectangular) coordinate system where the
    ///   coordinate axes, conventionally denoted the X axis and Y axis, are perpindicular to each other. For the
    ///   three-dimensional rectangular coordinate system, the third axis is called the Z axis.
    ///   </para>
    /// </remarks>
    /// <seealso cref="IEquatable{T}" />
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [DataContract]
    public struct PointF : IEquatable<PointF>
    {
        /// <summary>
        /// <see cref="PointF" /> with <see cref="X" /> and <see cref="Y" /> equal to <c>0f</c>.
        /// </summary>
        public static PointF Zero => default;

        /// <summary>
        /// <see cref="PointF" /> with <see cref="X" /> and <see cref="Y" /> set to not a number.
        /// </summary>
        public static PointF NaN => new PointF(float.NaN, float.NaN);

        /// <summary>
        /// The x-coordinate of this <see cref="PointF" />.
        /// </summary>
        [DataMember]
        public float X;

        /// <summary>
        /// The y-coordinate of this <see cref="PointF" />.
        /// </summary>
        [DataMember]
        public float Y;

        internal string DebuggerDisplay => string.Concat(
            X.ToString(), "  ",
            Y.ToString(), "  ");

        /// <summary>
        /// Initializes a new instance of the <see cref="PointF" /> structure from the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public PointF(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets a <see cref="Vector2"/> representation of this object.
        /// </summary>
        public readonly Vector2 ToVector2()
        {
            return Unsafe.BitCast<PointF, Vector2>(this);
        }

        /// <summary>
        /// Gets a <see cref="SizeF"/> representation of this object.
        /// </summary>
        public readonly SizeF ToSizeF()
        {
            return Unsafe.BitCast<PointF, SizeF>(this);
        }

        /// <summary>
        /// Gets a <see cref="Size"/> representation of this object.
        /// </summary>
        public readonly Size ToSize()
        {
            return ((Vector2)this).ToSize();
        }

        /// <summary>
        /// Gets a <see cref="Point"/> representation of this object.
        /// </summary>
        public readonly Point ToPoint()
        {
            return ((Vector2)this).ToPoint();
        }

        #region Add (operator +)

        /// <summary>
        /// Performs vector addition on <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The first vector to add.</param>
        /// <param name="b">The second vector to add.</param>
        /// <returns>The result of the vector addition.</returns>
        public static PointF Add(PointF a, PointF b) => a + b;

        /// <summary>
        /// Performs vector addition on <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">Source <see cref="PointF"/> on the left of the add sign.</param>
        /// <param name="b">Source <see cref="PointF"/> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static PointF operator +(PointF a, PointF b)
        {
            return (PointF)Vector2.Add(a, b);
        }

        #endregion

        #region Subtract (operator -)

        /// <summary>
        /// Performs vector subtraction on <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="left">Left <see cref="PointF"/>.</param>
        /// <param name="right">Right <see cref="PointF"/>.</param>
        /// <returns>The result of the vector subtraction.</returns>
        public static PointF Subtract(PointF left, PointF right) => left - right;

        /// <summary>
        /// Subtracts left from right.
        /// </summary>
        /// <param name="left">Left <see cref="PointF"/> on the left of the sub sign.</param>
        /// <param name="right">Right <see cref="PointF"/> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static PointF operator -(PointF left, PointF right)
        {
            return (PointF)Vector2.Subtract(left, right);
        }

        #endregion

        #region Multiply (operator *)

        public static PointF Multiply(PointF a, float b) => a * b;

        public static PointF operator *(PointF a, float b)
        {
            return (PointF)Vector2.Multiply(a, b);
        }

        #endregion

        #region Divide (operator /)

        public static PointF Divide(PointF left, float divisor) => left / divisor;

        public static PointF operator /(PointF left, float divisor)
        {
            return (PointF)Vector2.Divide(left, divisor);
        }

        #endregion

        /// <summary>
        /// Creates a new <see cref="PointF"/> that contains a minimal values from the two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The <see cref="PointF"/> with minimal values from the two points.</returns>
        public static PointF Min(PointF a, PointF b)
        {
            return (PointF)Vector2.Min(a, b);
        }

        /// <summary>
        /// Creates a new <see cref="PointF"/> that contains a maximal values from the two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The <see cref="PointF"/> with maximal values from the two points.</returns>
        public static PointF Max(PointF a, PointF b)
        {
            return (PointF)Vector2.Max(a, b);
        }

        #region Equals

        /// <summary>
        /// Indicates whether this <see cref="PointF" /> is equal to another <see cref="PointF" />.
        /// </summary>
        public readonly bool Equals(PointF other)
        {
            return this == other;
        }

        /// <summary>
        /// Returns a value indicating whether this <see cref="PointF" /> is equal to a specified object.
        /// </summary>
        public override readonly bool Equals(object? obj)
        {
            return obj is PointF other && Equals(other);
        }

        /// <summary>
        /// Compares two <see cref="PointF" /> structures. The result specifies
        /// whether the values of the <see cref="X" /> and <see cref="Y" />
        /// fields of the two <see cref="PointF" /> structures are equal.
        /// </summary>
        public static bool operator ==(PointF a, PointF b)
        {
            return (a.X == b.X) && (a.Y == b.Y);
        }

        /// <summary>
        /// Compares two <see cref="PointF" /> structures. The result specifies
        /// whether the values of the <see cref="X" /> or <see cref="Y" />
        /// fields of the two <see cref="PointF" /> structures are unequal.
        /// </summary>
        public static bool operator !=(PointF first, PointF second)
        {
            return !(first == second);
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a hash code of this <see cref="PointF" />.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// Returns a <see cref="string" /> that represents this <see cref="PointF" />.
        /// </summary>
        public override readonly string ToString() => $"{{X:{X}, Y:{Y}}}";

        #endregion

        #region Explicit operators

        public static explicit operator PointF(Vector2 vector)
        {
            return vector.ToPointF();
        }

        #endregion

        #region Implicit operators

        public static implicit operator Vector2(PointF point)
        {
            return point.ToVector2();
        }

        public static implicit operator PointF(SizeF size)
        {
            return size.ToPointF();
        }

        #endregion
    }
}
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MonoGame.Framework
{
    /// <summary>
    /// A two dimensional size defined by two real numbers, a width and a height.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A size is a subspace of two-dimensional space, the area of which is described in terms of a two-dimensional
    /// coordinate system, given by a reference point and two coordinate axes.
    /// </para>
    /// </remarks>
    /// <seealso cref="IEquatable{T}" />
    [DataContract]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Size : IEquatable<Size>
    {
        // TODO: add float operators that return a SizeF

        /// <summary>
        /// Returns a <see cref="Size" /> with <see cref="Width" /> and <see cref="Height" /> equal to <c>0f</c>.
        /// </summary>
        public static Size Empty => default;

        /// <summary>
        /// The horizontal component of this <see cref="Size" />.
        /// </summary>
        [DataMember] public int Width;

        /// <summary>
        /// The vertical component of this <see cref="Size" />.
        /// </summary>
        [DataMember] public int Height;

        /// <summary>
        /// Gets a value that indicates whether this <see cref="Size" /> is empty.
        /// </summary>
        public readonly bool IsEmpty => Width == 0 && Height == 0;

        public readonly int Area => Width * Height;

        internal string DebuggerDisplay => $"Width = {Width}, Height = {Height}";

        /// <summary>
        /// Initializes a new instance of the <see cref="Size" /> structure from the specified dimensions.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Size" /> structure with one value for all dimensions.
        /// </summary>
        /// <param name="value">The width and height.</param>
        public Size(int value) : this(value, value)
        {
        }

        /// <summary>
        /// Gets a <see cref="Point"/> representation for this object.
        /// </summary>
        public readonly Point ToPoint()
        {
            return Unsafe.BitCast<Size, Point>(this);
        }

        /// <summary>
        /// Gets a <see cref="Point"/> representation for this object.
        /// </summary>
        public readonly PointF ToPointF()
        {
            return new PointF(Width, Height);
        }

        /// <summary>
        /// Gets a <see cref="SizeF"/> representation for this object.
        /// </summary>
        public readonly SizeF ToSizeF()
        {
            return new SizeF(Width, Height);
        }

        /// <summary>
        /// Gets a <see cref="Vector2"/> representation for this object.
        /// </summary>
        public readonly Vector2 ToVector2()
        {
            return new Vector2(Width, Height);
        }

        public readonly Rectangle ToRect()
        {
            return new Rectangle(0, 0, Width, Height);
        }

        /// <summary>
        /// Calculates the <see cref="Size" /> representing the vector addition of two <see cref="Size" /> structures.
        /// </summary>
        /// <param name="a">The first size.</param>
        /// <param name="b">The second size.</param>
        /// <returns>
        /// The <see cref="Size" /> representing the vector addition of two <see cref="Size" /> structures.
        /// </returns>
        public static Size Add(Size a, Size b) => a + b;

        /// <summary>
        /// Calculates the <see cref="Size" /> representing the vector subtraction of two <see cref="Size" /> structures.
        /// </summary>
        /// <param name="left">The first size.</param>
        /// <param name="right">The second size.</param>
        /// <returns>
        /// The <see cref="Size" /> representing the vector subtraction of two <see cref="Size" /> structures.
        /// </returns>
        public static Size Subtract(Size left, Size right) => left - right;

        public static Size Multiply(Size a, int b) => a * b;

        public static SizeF Multiply(Size a, float b) => a * b;

        public static Size Divide(Size left, int divisor) => left / divisor;

        public static SizeF Divide(Size left, float disivor) => left / disivor;

        #region Operators

        /// <summary>
        /// Calculates the <see cref="Size" /> representing the vector addition of two <see cref="Size" /> structures as if
        /// they were <see cref="Vector2" /> structures.
        /// </summary>
        /// <param name="a">The first size.</param>
        /// <param name="b">The second size.</param>
        /// <returns>The <see cref="Size" /> representing the vector addition.</returns>
        public static Size operator +(Size a, Size b)
        {
            return new Size(a.Width + b.Width, a.Height + b.Height);
        }

        /// <summary>
        /// Calculates the <see cref="Size" /> representing the vector subtraction.
        /// </summary>
        /// <param name="left">The first size.</param>
        /// <param name="right">The second size.</param>
        /// <returns>The <see cref="Size" /> representing the vector subtraction.</returns>
        public static Size operator -(Size left, Size right)
        {
            return new Size(left.Width - right.Width, left.Height - right.Height);
        }

        public static Size operator *(Size a, int b)
        {
            return new Size(a.Width * b, a.Height * b);
        }

        public static SizeF operator *(Size a, float b)
        {
            return (SizeF)a * b;
        }

        public static Size operator /(Size left, int divisor)
        {
            return new Size(left.Width / divisor, left.Height / divisor);
        }

        public static SizeF operator /(Size left, float divisor)
        {
            return (SizeF)left / divisor;
        }

        #endregion

        #region Equals (operator ==, !=)

        /// <summary>
        /// Indicates whether this <see cref="Size" /> is equal to another <see cref="Size" />.
        /// </summary>
        public readonly bool Equals(Size other) => this == other;

        /// <summary>
        /// Returns a value indicating whether this <see cref="Size" /> is equal to a specified object.
        /// </summary>
        public override readonly bool Equals(object? obj) => obj is Size size && Equals(size);

        /// <summary>
        /// Compares two <see cref="Size" /> structures. The result specifies
        /// whether the values of the <see cref="Width" /> and <see cref="Height" />
        /// fields of the two <see cref="Size" /> structures are equal.
        /// </summary>
        public static bool operator ==(Size a, Size b)
        {
            return (a.Width == b.Width)
                && (a.Height == b.Height);
        }

        /// <summary>
        /// Compares two <see cref="Size" /> structures. The result specifies
        /// whether the values of the <see cref="Width" /> or <see cref="Height" />
        /// fields of the two <see cref="Size" /> structures are unequal.
        /// </summary>
        public static bool operator !=(Size a, Size b)
        {
            return !(a == b);
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a <see cref="string" /> that represents this <see cref="Size" />.
        /// </summary>
        public override readonly string ToString() => $"{{W:{Width}, H:{Height}}}";

        /// <summary>
        /// Returns a hash code of this <see cref="Size" />.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(Width, Height);

        #endregion

        #region Explicit operators

        public static explicit operator Size(Vector2 vector)
        {
            return vector.ToSize();
        }

        public static explicit operator Size(PointF point)
        {
            return point.ToSize();
        }

        public static explicit operator Size(SizeF size)
        {
            return size.ToSize();
        }

        #endregion

        #region Implicit Operators

        public static implicit operator Vector2(Size size)
        {
            return size.ToVector2();
        }

        public static implicit operator PointF(Size size)
        {
            return size.ToPointF();
        }

        public static implicit operator SizeF(Size size)
        {
            return size.ToSizeF();
        }

        #endregion
    }
}
using System;
using System.Diagnostics;
using System.Numerics;
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
    public struct SizeF : IEquatable<SizeF>
    {
        /// <summary>
        /// Returns a <see cref="SizeF" /> with <see cref="Width" /> and <see cref="Height" /> equal to <c>0f</c>.
        /// </summary>
        public static SizeF Empty => default;

        /// <summary>
        /// The horizontal component of this <see cref="SizeF" />.
        /// </summary>
        [DataMember]
        public float Width;

        /// <summary>
        /// The vertical component of this <see cref="SizeF" />.
        /// </summary>
        [DataMember]
        public float Height;

        /// <summary>
        /// Gets a value that indicates whether this <see cref="SizeF" /> is empty.
        /// </summary>
        public bool IsEmpty => (Width == 0) && (Height == 0);

        internal string DebuggerDisplay => $"Width = {Width}, Height = {Height}";

        /// <summary>
        /// Initializes a new instance of the <see cref="SizeF" /> structure from the specified dimensions.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public SizeF(float width, float height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SizeF" /> structure from the specified dimensions.
        /// </summary>
        /// <param name="value">The width and height.</param>
        public SizeF(float value) : this(value, value)
        {
        }

        /// <summary>
        /// Gets a <see cref="Vector2"/> representation of this object.
        /// </summary>
        public readonly Vector2 ToVector2()
        {
            return Unsafe.BitCast<SizeF, Vector2>(this);
        }

        /// <summary>
        /// Gets a <see cref="PointF"/> representation of this object.
        /// </summary>
        public readonly PointF ToPointF()
        {
            return Unsafe.BitCast<SizeF, PointF>(this);
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

        /// <summary>
        /// Calculates the <see cref="SizeF" /> representing the vector addition.
        /// </summary>
        /// <param name="a">The first size.</param>
        /// <param name="b">The second size.</param>
        /// <returns>The <see cref="SizeF" /> representing the vector addition.</returns>
        public static SizeF Add(SizeF a, SizeF b)
        {
            return a + b;
        }

        /// <summary>
        /// Calculates the <see cref="SizeF" /> representing the vector subtraction.
        /// </summary>
        /// <param name="left">The first size.</param>
        /// <param name="right">The second size.</param>
        /// <returns>The <see cref="SizeF" /> representing the vector subtraction.</returns>
        public static SizeF Subtract(SizeF left, SizeF right)
        {
            return left - right;
        }

        public static SizeF Multiply(SizeF a, float b)
        {
            return a * b;
        }

        public static SizeF Divide(SizeF left, float divisor)
        {
            return left / divisor;
        }

        /// <summary>
        /// Calculates the <see cref="SizeF" /> representing the vector addition of two <see cref="SizeF" /> as if
        /// they were <see cref="Vector2" /> structures.
        /// </summary>
        /// <param name="a">The first size.</param>
        /// <param name="b">The second size.</param>
        /// <returns>
        /// The <see cref="SizeF" /> representing the vector addition of two <see cref="SizeF" /> structures as if they
        /// were <see cref="Vector2" /> structures.
        /// </returns>
        public static SizeF operator +(SizeF a, SizeF b)
        {
            return (SizeF)Vector2.Add(a, b);
        }

        /// <summary>
        /// Calculates the <see cref="SizeF" /> representing the vector subtraction of two <see cref="SizeF" /> structures.
        /// </summary>
        /// <param name="left">The first size.</param>
        /// <param name="right">The second size.</param>
        /// <returns>
        /// The <see cref="SizeF" /> representing the vector subtraction of two <see cref="SizeF" /> structures.
        /// </returns>
        public static SizeF operator -(SizeF left, SizeF right)
        {
            return (SizeF)Vector2.Subtract(left, right);
        }

        public static SizeF operator *(SizeF a, float b)
        {
            return (SizeF)Vector2.Multiply(a, b);
        }

        public static SizeF operator /(SizeF left, float divisor)
        {
            return (SizeF)Vector2.Divide(left, divisor);
        }

        #region Equals

        /// <summary>
        /// Indicates whether this <see cref="SizeF" /> is equal to another <see cref="SizeF" />.
        /// </summary>
        public readonly bool Equals(SizeF other)
        {
            return this == other;
        }

        /// <summary>
        /// Returns a value indicating whether this <see cref="SizeF" /> is equal to a specified object.
        /// </summary>
        public override readonly bool Equals(object? obj)
        {
            return obj is SizeF other && Equals(other);
        }

        /// <summary>
        /// Compares two <see cref="SizeF" /> structures. The result specifies
        /// whether the values of the <see cref="Width" /> and <see cref="Height" />
        /// fields of the two <see cref="PointF" /> structures are equal.
        /// </summary>
        public static bool operator ==(SizeF a, SizeF b)
        {
            return (a.Width, a.Height) == (b.Width, b.Height);
        }

        /// <summary>
        /// Compares two <see cref="SizeF" /> structures. The result specifies
        /// whether the values of the <see cref="Width" /> or <see cref="Height" />
        /// fields of the two <see cref="SizeF" /> structures are unequal.
        /// </summary>
        public static bool operator !=(SizeF a, SizeF b)
        {
            return (a.Width, a.Height) != (b.Width, b.Height);
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a <see cref="string" /> that represents this <see cref="SizeF" />.
        /// </summary>
        public override readonly string ToString() => $"{{W:{Width}, H:{Height}}}";

        /// <summary>
        /// Returns a hash code of this <see cref="SizeF" />.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(Width, Height);

        #endregion

        #region Explicit operators

        public static explicit operator SizeF(Vector2 vector)
        {
            return vector.ToSizeF();
        }

        #endregion

        #region Implicit operators

        public static implicit operator Vector2(SizeF size)
        {
            return size.ToVector2();
        }

        #endregion
    }
}
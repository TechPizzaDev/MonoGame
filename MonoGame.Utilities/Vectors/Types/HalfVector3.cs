// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing 16-bit floating-point XYZ components.
    /// <para>Ranges from [-1, -1, -1, 1] to [1, 1, 1, 1] in vector form.</para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HalfVector3 : IPixel<HalfVector3>
    {
        public static HalfVector3 Zero => default;
        public static HalfVector3 One { get; } = new HalfVector3(HalfSingle.One);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Blue));

        public HalfSingle X;
        public HalfSingle Y;
        public HalfSingle Z;

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with raw values.
        /// </summary>
        public HalfVector3(HalfSingle x, HalfSingle y, HalfSingle z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Constructs the packed vector with a raw value.
        /// </summary>
        public HalfVector3(HalfSingle value) : this(value, value, value)
        {
        }

        #endregion

        #region IPackedVector

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector *= 2;
            scaledVector -= Vector3.One;

            X = (HalfSingle)scaledVector.X;
            Y = (HalfSingle)scaledVector.Y;
            Z = (HalfSingle)scaledVector.Z;
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector)
        {
            X = (HalfSingle)vector.X;
            Y = (HalfSingle)vector.Y;
            Z = (HalfSingle)vector.Z;
        }

        public void FromVector(Vector4 vector) => FromVector(vector.AsVector3());

        public readonly Vector3 ToScaledVector3()
        {
            var vector = ToVector3();
            vector += Vector3.One;
            vector /= 2f;
            return vector;
        }

        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => new Vector3(X, Y, Z);
        public readonly Vector4 ToVector4() => new Vector4(ToVector3(), 1);

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => this = One;
        public void FromAlpha(Alpha16 source) => this = One;
        public void FromAlpha(Alpha32 source) => this = One;
        public void FromAlpha(AlphaF source) => this = One;

        public void FromGray(Gray8 source) => FromColor(source.ToRgb24());
        public void FromGray(Gray16 source) => FromColor(source.ToRgb48());
        public void FromGray(Gray32 source) => FromScaledVector(source.ToScaledVector3());
        public void FromGray(GrayF source) => FromScaledVector(source.ToScaledVector3());
        public void FromGray(GrayAlpha16 source) => FromColor(source.ToRgba32());

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());
        public void FromColor(Bgr24 source) => FromColor(source.ToRgb24());
        public void FromColor(Rgb24 source) => FromScaledVector(source.ToScaledVector3());
        public void FromColor(Rgb48 source) => FromScaledVector(source.ToScaledVector3());

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());
        public void FromColor(Abgr32 source) => FromColor(source.ToRgba32());
        public void FromColor(Argb32 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra32 source) => FromColor(source.ToRgba32());
        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());
        public void FromColor(Color source) => FromScaledVector(source.ToScaledVector4());
        public void FromColor(Rgba64 source) => FromScaledVector(source.ToScaledVector4());

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => Alpha8.Opaque;
        public readonly Alpha16 ToAlpha16() => Alpha16.Opaque;
        public readonly AlphaF ToAlphaF() => AlphaF.Opaque;

        public readonly Gray8 ToGray8() => PixelHelper.ToGray8(this);
        public readonly Gray16 ToGray16() => PixelHelper.ToGray16(this);
        public readonly GrayF ToGrayF() => PixelHelper.ToGrayF(this);
        public readonly GrayAlpha16 ToGrayAlpha16() => PixelHelper.ToGrayAlpha16(this);

        public readonly Rgb24 ToRgb24() => ScaledVectorHelper.ToRgb24(this);
        public readonly Rgb48 ToRgb48() => ScaledVectorHelper.ToRgb48(this);

        public readonly Color ToRgba32() => ScaledVectorHelper.ToRgba32(this);
        public readonly Rgba64 ToRgba64() => ScaledVectorHelper.ToRgba64(this);

        #endregion

        #region Equals

        public readonly bool Equals(HalfVector3 other) => this == other;

        /// <summary>
        /// Compares the current instance to another to determine whether they are the same.
        /// </summary>
        public static bool operator ==(HalfVector3 a, HalfVector3 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        /// <summary>
        /// Compares the current instance to another to determine whether they are different.
        /// </summary>
        public static bool operator !=(in HalfVector3 a, in HalfVector3 b) => !(a == b);

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        public override readonly bool Equals(object? obj) => obj is HalfVector3 other && Equals(other);

        /// <summary>
        /// Gets the hash code for the current instance.
        /// </summary>
        // Use PackedValue so we don't call the diffused HalfSingle.GetHashCode
        public override readonly int GetHashCode() => HashCode.Combine(X.PackedValue, Y.PackedValue, Z.PackedValue);

        /// <summary>
        /// Returns a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(HalfVector3) + $"({ToVector3()})";

        #endregion
    }
}

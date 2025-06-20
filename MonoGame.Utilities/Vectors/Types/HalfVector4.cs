// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing 16-bit floating-point XYZW components.
    /// <para>Ranges from [-1, -1, -1, 0] to [1, 1, 1, 1] in vector form.</para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HalfVector4 : IPixel<HalfVector4>, IPackedVector<ulong>
    {
        public static HalfVector4 Zero => default;
        public static HalfVector4 One { get; } = new HalfVector4(HalfSingle.One);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Blue),
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Alpha));

        public HalfSingle X;
        public HalfSingle Y;
        public HalfSingle Z;
        public HalfSingle W;

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with raw values.
        /// </summary>
        public HalfVector4(HalfSingle x, HalfSingle y, HalfSingle z, HalfSingle w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Constructs the packed vector with a raw value.
        /// </summary>
        public HalfVector4(HalfSingle value) : this(value, value, value, value)
        {
        }

        /// <summary>
        /// Constructs the packed vector with a packed value.
        /// </summary>
        [CLSCompliant(false)]
        public HalfVector4(ulong packed) : this()
        {
            // TODO: Unsafe.SkipInit(out this)
            PackedValue = packed;
        }

        #endregion

        #region IPackedVector

        [CLSCompliant(false)]
        public ulong PackedValue
        {
            readonly get => Unsafe.BitCast<HalfVector4, ulong>(this);
            set => Unsafe.As<HalfVector4, ulong>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector *= 2;
            scaledVector -= Vector3.One;

            X = (HalfSingle)scaledVector.X;
            Y = (HalfSingle)scaledVector.Y;
            Z = (HalfSingle)scaledVector.Z;
            W = HalfSingle.One;
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector *= 2;
            scaledVector -= Vector4.One;

            X = (HalfSingle)scaledVector.X;
            Y = (HalfSingle)scaledVector.Y;
            Z = (HalfSingle)scaledVector.Z;
            W = (HalfSingle)scaledVector.W;
        }

        public void FromVector(Vector3 vector)
        {
            X = (HalfSingle)vector.X;
            Y = (HalfSingle)vector.Y;
            Z = (HalfSingle)vector.Z;
            W = HalfSingle.One;
        }

        public void FromVector(Vector4 vector)
        {
            X = (HalfSingle)vector.X;
            Y = (HalfSingle)vector.Y;
            Z = (HalfSingle)vector.Z;
            W = (HalfSingle)vector.W;
        }

        public readonly Vector3 ToScaledVector3()
        {
            var vector = ToVector3();
            vector += Vector3.One;
            vector /= 2f;
            return vector;
        }

        public readonly Vector4 ToScaledVector4()
        {
            var vector = ToVector4();
            vector += Vector4.One;
            vector /= 2f;
            return vector;
        }

        public readonly Vector3 ToVector3() => new Vector3(X, Y, Z);
        public readonly Vector4 ToVector4() => new Vector4(ToVector3(), W);

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            X = Y = Z = HalfSingle.One;
            W = HalfSingle.FromScaled(ScalingHelper.ToFloat32(source.A));
        }

        public void FromAlpha(Alpha16 source)
        {
            X = Y = Z = HalfSingle.One;
            W = HalfSingle.FromScaled(ScalingHelper.ToFloat32(source.A));
        }

        public void FromAlpha(Alpha32 source)
        {
            X = Y = Z = HalfSingle.One;
            W = HalfSingle.FromScaled(ScalingHelper.ToFloat32(source.A));
        }

        public void FromAlpha(AlphaF source)
        {
            X = Y = Z = HalfSingle.One;
            W = HalfSingle.FromScaled(source.A);
        }

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

        public readonly Alpha8 ToAlpha8() => ScalingHelper.ToUInt8(W.ToScaledSingle());
        public readonly Alpha16 ToAlpha16() => ScalingHelper.ToUInt16(W.ToScaledSingle());
        public readonly AlphaF ToAlphaF() => W.ToScaledSingle();

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

        [CLSCompliant(false)]
        public readonly bool Equals(ulong other) => PackedValue == other;

        public readonly bool Equals(HalfVector4 other) => this == other;

        /// <summary>
        /// Compares the current instance to another to determine whether they are the same.
        /// </summary>
        public static bool operator ==(HalfVector4 a, HalfVector4 b) => a.PackedValue == b.PackedValue;

        /// <summary>
        /// Compares the current instance to another to determine whether they are different.
        /// </summary>
        public static bool operator !=(HalfVector4 a, HalfVector4 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        public override readonly bool Equals(object? obj) => obj is HalfVector4 other && Equals(other);

        /// <summary>
        /// Gets the hash code for the current instance.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Returns a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(HalfVector4) + $"({ToVector4()})";

        #endregion
    }
}

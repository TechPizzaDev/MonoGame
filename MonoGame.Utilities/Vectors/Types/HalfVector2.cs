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
    /// Packed vector type containing 16-bit floating-point XY components.
    /// <para>
    /// Ranges from [-1, -1, 0, 1] to [1, 1, 0, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HalfVector2 : IPixel<HalfVector2>, IPackedVector<uint>
    {
        public static HalfVector2 Zero => default;
        public static HalfVector2 One { get; } = new HalfVector2(HalfSingle.One);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Green));

        public HalfSingle X;
        public HalfSingle Y;

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with raw values.
        /// </summary>
        public HalfVector2(HalfSingle x, HalfSingle y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructs the packed vector with a raw value.
        /// </summary>
        public HalfVector2(HalfSingle value) : this(value, value)
        {
        }

        /// <summary>
        /// Constructs the packed vector with a packed value.
        /// </summary>
        [CLSCompliant(false)]
        public HalfVector2(uint packed) : this()
        {
            // TODO: Unsafe.SkipInit(out this)
            PackedValue = packed;
        }

        #endregion

        public readonly Vector2 ToVector2() => new Vector2(X, Y);

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<HalfVector2, uint>(this);
            set => Unsafe.As<HalfVector2, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            var vector = scaledVector.AsVector2();
            vector *= 2;
            vector -= Vector2.One;

            X = (HalfSingle)vector.X;
            Y = (HalfSingle)vector.Y;
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector)
        {
            X = (HalfSingle)vector.X;
            Y = (HalfSingle)vector.Y;
        }

        public void FromVector(Vector4 vector) => FromVector(vector.AsVector3());

        public readonly Vector3 ToScaledVector3()
        {
            var vector = ToVector2();
            vector += Vector2.One;
            vector /= 2f;
            return new Vector3(vector, 0);
        }

        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => new Vector3(X, Y, 0);
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

        [CLSCompliant(false)]
        public readonly bool Equals(uint other) => PackedValue == other;

        public readonly bool Equals(HalfVector2 other) => this == other;

        public static bool operator ==(HalfVector2 a, HalfVector2 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(HalfVector2 a, HalfVector2 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is HalfVector2 other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        public override readonly string ToString() => nameof(HalfVector2) + $"({ToVector2()})";

        #endregion
    }
}

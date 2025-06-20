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
    /// Packed vector type containing signed 8-bit XY components.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 0, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rg16 : IPixel<Rg16>, IPackedVector<ushort>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Green));

        [CLSCompliant(false)]
        public byte R;

        [CLSCompliant(false)]
        public byte G;

        #region Constructors

        [CLSCompliant(false)]
        public Rg16(byte x, byte y)
        {
            R = x;
            G = y;
        }

        [CLSCompliant(false)]
        public Rg16(ushort packed) : this()
        {
            PackedValue = packed;
        }

        #endregion

        /// <summary>
        /// Gets the packed vector in <see cref="Vector2"/> format.
        /// </summary>
        public readonly Vector2 ToScaledVector2() => new Vector2(R, G) / byte.MaxValue;

        #region IPackedVector

        [CLSCompliant(false)]
        public ushort PackedValue
        {
            readonly get => Unsafe.BitCast<Rg16, ushort>(this);
            set => Unsafe.As<Rg16, ushort>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            var vector = scaledVector.AsVector2();
            vector = VectorHelper.ScaledClamp(vector);
            vector *= byte.MaxValue;
            vector += new Vector2(0.5f);

            R = (byte)vector.X;
            G = (byte)vector.Y;
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(ToScaledVector2(), 0);
        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => R = G = byte.MaxValue;
        public void FromAlpha(Alpha16 source) => R = G = byte.MaxValue;
        public void FromAlpha(Alpha32 source) => R = G = byte.MaxValue;
        public void FromAlpha(AlphaF source) => R = G = byte.MaxValue;

        public void FromGray(Gray8 source) => R = G = source.L;
        public void FromGray(Gray16 source) => R = G = ScalingHelper.ToUInt8(source.L);
        public void FromGray(Gray32 source) => R = G = ScalingHelper.ToUInt8(source.L);
        public void FromGray(GrayF source) => R = G = ScalingHelper.ToUInt8(source.L);
        public void FromGray(GrayAlpha16 source) => R = G = source.L;

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            R = source.R;
            G = source.G;
        }

        public void FromColor(Rgb24 source)
        {
            R = source.R;
            G = source.G;
        }

        public void FromColor(Rgb48 source)
        {
            R = ScalingHelper.ToUInt8(source.R);
            G = ScalingHelper.ToUInt8(source.G);
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            R = source.R;
            G = source.G;
        }

        public void FromColor(Argb32 source)
        {
            R = source.R;
            G = source.G;
        }

        public void FromColor(Bgra32 source)
        {
            R = source.R;
            G = source.G;
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            R = source.R;
            G = source.G;
        }

        public void FromColor(Rgba64 source)
        {
            R = ScalingHelper.ToUInt8(source.R);
            G = ScalingHelper.ToUInt8(source.G);
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => Alpha8.Opaque;
        public readonly Alpha16 ToAlpha16() => Alpha16.Opaque;
        public readonly AlphaF ToAlphaF() => AlphaF.Opaque;

        public readonly Gray8 ToGray8() => PixelHelper.ToGray8(R, G, 0);
        public readonly Gray16 ToGray16() => PixelHelper.ToGray16(this);
        public readonly GrayF ToGrayF() => PixelHelper.ToGrayF(this);
        public readonly GrayAlpha16 ToGrayAlpha16() => PixelHelper.ToGrayAlpha16(R, G, 0);

        public readonly Rgb24 ToRgb24() => new Rgb24(R, G, 0);
        public readonly Rgb48 ToRgb48() => ToRgb24().ToRgb48();

        public readonly Color ToRgba32() => new Color(R, G, (byte)0);
        public readonly Rgba64 ToRgba64() => ToRgba32().ToRgba64();

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(ushort other) => PackedValue == other;

        public readonly bool Equals(Rg16 other) => this == other;

        public static bool operator ==(Rg16 a, Rg16 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(Rg16 a, Rg16 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Rg16 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Rg16) + $"(R:{R}, G:{G})";

        #endregion
    }
}

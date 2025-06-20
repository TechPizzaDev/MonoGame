// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing a 8-bit XYZ luminance.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Gray8 : IPixel<Gray8>, IPackedVector<byte>
    {
        public static Gray8 White => new Gray8(byte.MaxValue);

        VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Luminance));

        public byte L;

        public Gray8(byte luminance)
        {
            L = luminance;
        }

        #region IPackedVector

        public byte PackedValue
        {
            readonly get => L;
            set => L = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= byte.MaxValue;
            scaledVector += new Vector3(0.5f);

            L = (byte)(PixelHelper.ToGrayF(scaledVector) + 0.5f);
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3()
        {
            float l = L / (float)byte.MaxValue;
            return new Vector3(l, l, l);
        }

        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1f);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => L = byte.MaxValue;
        public void FromAlpha(Alpha16 source) => L = byte.MaxValue;
        public void FromAlpha(Alpha32 source) => L = byte.MaxValue;
        public void FromAlpha(AlphaF source) => L = byte.MaxValue;

        public void FromGray(Gray8 source) => L = source.L;
        public void FromGray(Gray16 source) => L = ScalingHelper.ToUInt8(source.L);
        public void FromGray(Gray32 source) => L = ScalingHelper.ToUInt8(source.L);
        public void FromGray(GrayF source) => L = ScalingHelper.ToUInt8(source.L);
        public void FromGray(GrayAlpha16 source) => L = source.L;

        public void FromColor(Bgr565 source) => L = PixelHelper.ToGray8(source);
        public void FromColor(Bgr24 source) => L = PixelHelper.ToGray8(source.R, source.G, source.B);
        public void FromColor(Rgb24 source) => L = PixelHelper.ToGray8(source);
        public void FromColor(Rgb48 source) => L = PixelHelper.ToGray8(source);

        public void FromColor(Bgra4444 source) => L = PixelHelper.ToGray8(source);
        public void FromColor(Bgra5551 source) => L = PixelHelper.ToGray8(source);
        public void FromColor(Argb32 source) => L = PixelHelper.ToGray8(source.R, source.G, source.B);
        public void FromColor(Abgr32 source) => L = PixelHelper.ToGray8(source.R, source.G, source.B);
        public void FromColor(Bgra32 source) => L = PixelHelper.ToGray8(source.R, source.G, source.B);
        public void FromColor(Rgba1010102 source) => L = PixelHelper.ToGray8(source);
        public void FromColor(Color source) => L = PixelHelper.ToGray8(source.R, source.G, source.B);
        public void FromColor(Rgba64 source) => L = PixelHelper.ToGray8(source.Rgb);

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => Alpha8.Opaque;
        public readonly Alpha16 ToAlpha16() => Alpha16.Opaque;
        public readonly AlphaF ToAlphaF() => AlphaF.Opaque;

        public readonly Gray8 ToGray8() => this;
        public readonly Gray16 ToGray16() => ScalingHelper.ToUInt16(L);
        public readonly GrayF ToGrayF() => ScalingHelper.ToFloat32(L);
        public readonly GrayAlpha16 ToGrayAlpha16() => new GrayAlpha16(L, byte.MaxValue);

        public readonly Rgb24 ToRgb24() => new Rgb24(L);
        public readonly Rgb48 ToRgb48() => new Rgb48(ScalingHelper.ToUInt16(L));

        public readonly Color ToRgba32() => new Color(L, byte.MaxValue);
        public readonly Rgba64 ToRgba64() => new Rgba64(ScalingHelper.ToUInt16(L), ushort.MaxValue);

        #endregion

        #region Equals

        public readonly bool Equals(byte other) => PackedValue == other;
        public readonly bool Equals(Gray8 other) => this == other;

        public static bool operator ==(Gray8 a, Gray8 b) => a.L == b.L;
        public static bool operator !=(Gray8 a, Gray8 b) => a.L != b.L;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Gray8 other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(L);

        public override readonly string ToString() => nameof(Gray8) + $"({L})";

        #endregion

        public static implicit operator Gray8(byte luminance) => new Gray8(luminance);
        public static implicit operator byte(Gray8 value) => value.L;
    }
}

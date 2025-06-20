// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing a 32-bit XYZ luminance.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Gray32 : IPixel<Gray32>, IPackedVector<uint>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt32, VectorComponentChannel.Luminance));

        [CLSCompliant(false)]
        public uint L;

        [CLSCompliant(false)]
        public Gray32(uint luminance)
        {
            L = luminance;
        }

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => L;
            set => L = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= uint.MaxValue;
            scaledVector += new Vector3(0.5f);

            L = PixelHelper.ToGray32(scaledVector);
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(ScalingHelper.ToFloat32(L));
        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => L = uint.MaxValue;
        public void FromAlpha(Alpha16 source) => L = uint.MaxValue;
        public void FromAlpha(Alpha32 source) => L = uint.MaxValue;
        public void FromAlpha(AlphaF source) => L = uint.MaxValue;

        public void FromGray(Gray8 source) => L = ScalingHelper.ToUInt32(source.L);
        public void FromGray(Gray16 source) => L = ScalingHelper.ToUInt32(source.L);
        public void FromGray(Gray32 source) => this = source;
        public void FromGray(GrayF source) => L = ScalingHelper.ToUInt32(source.L);
        public void FromGray(GrayAlpha16 source) => L = ScalingHelper.ToUInt32(source.L);

        public void FromColor(Bgr565 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Bgr24 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Rgb24 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Rgb48 source) => L = PixelHelper.ToGray32(source);

        public void FromColor(Bgra4444 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Bgra5551 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Argb32 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Abgr32 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Bgra32 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Rgba1010102 source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Color source) => L = PixelHelper.ToGray32(source);
        public void FromColor(Rgba64 source) => L = PixelHelper.ToGray32(source);

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => Alpha8.Opaque;
        public readonly Alpha16 ToAlpha16() => Alpha16.Opaque;
        public readonly AlphaF ToAlphaF() => AlphaF.Opaque;

        public readonly Gray8 ToGray8() => ScalingHelper.ToUInt8(L);
        public readonly Gray16 ToGray16() => ScalingHelper.ToUInt16(L);
        public readonly GrayAlpha16 ToGrayAlpha16() => new GrayAlpha16(ScalingHelper.ToUInt8(L), byte.MaxValue);
        public readonly GrayF ToGrayF() => ScalingHelper.ToFloat32(L);

        public readonly Rgb24 ToRgb24() => new Rgb24(ScalingHelper.ToUInt8(L));
        public readonly Rgb48 ToRgb48() => new Rgb48(ScalingHelper.ToUInt16(L));

        public readonly Color ToRgba32() => new Color(ScalingHelper.ToUInt8(L), byte.MaxValue);
        public readonly Rgba64 ToRgba64() => new Rgba64(ScalingHelper.ToUInt16(L), ushort.MaxValue);

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(uint other) => PackedValue == other;

        public readonly bool Equals(Gray32 other) => this == other;

        public static bool operator ==(Gray32 a, Gray32 b) => a.L == b.L;
        public static bool operator !=(Gray32 a, Gray32 b) => a.L != b.L;

        #endregion

        #region Object Overrides

        public override readonly bool Equals(object? obj) => obj is Gray32 other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(L);

        public override readonly string ToString() => nameof(Gray32) + $"({L})";

        #endregion

        [CLSCompliant(false)]
        public static implicit operator Gray32(uint luminance) => new Gray32(luminance);

        [CLSCompliant(false)]
        public static implicit operator uint(Gray32 value) => value.L;
    }
}
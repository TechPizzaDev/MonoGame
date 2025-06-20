// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing an 8-bit XYZ luminance and 8-bit W component.
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GrayAlpha16 : IPixel<GrayAlpha16>, IPackedVector<ushort>
    {
        public static GrayAlpha16 OpaqueWhite => new GrayAlpha16(byte.MaxValue, byte.MaxValue);
        public static GrayAlpha16 TransparentWhite => new GrayAlpha16(byte.MaxValue, 0);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Luminance),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Alpha));

        public byte L;
        public byte A;

        public GrayAlpha16(byte luminance, byte alpha)
        {
            L = luminance;
            A = alpha;
        }

        [CLSCompliant(false)]
        public GrayAlpha16(ushort packedValue) : this()
        {
            PackedValue = packedValue;
        }

        #region IPackedVector

        [CLSCompliant(false)]
        public ushort PackedValue
        {
            readonly get => Unsafe.BitCast<GrayAlpha16, ushort>(this);
            set => Unsafe.As<GrayAlpha16, ushort>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= byte.MaxValue;
            scaledVector += new Vector3(0.5f);

            L = (byte)(PixelHelper.ToGrayF(scaledVector) + 0.5f);
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= byte.MaxValue;
            scaledVector += new Vector4(0.5f);

            L = (byte)(PixelHelper.ToGrayF(scaledVector.AsVector3()) + 0.5f);
            A = (byte)scaledVector.W;
        }

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(L) / byte.MaxValue;
        public readonly Vector4 ToScaledVector4() => new Vector4(L, L, L, A) / byte.MaxValue;

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            L = byte.MaxValue;
            A = source.A;
        }

        public void FromAlpha(Alpha16 source)
        {
            L = byte.MaxValue;
            A = ScalingHelper.ToUInt8(source.A);
        }

        public void FromAlpha(Alpha32 source)
        {
            L = byte.MaxValue;
            A = ScalingHelper.ToUInt8(source.A);
        }

        public void FromAlpha(AlphaF source)
        {
            L = byte.MaxValue;
            A = ScalingHelper.ToUInt8(source.A);
        }

        public void FromGray(Gray8 source)
        {
            L = source.L;
            A = byte.MaxValue;
        }

        public void FromGray(Gray16 source)
        {
            L = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(Gray32 source)
        {
            L = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(GrayF source)
        {
            L = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(GrayAlpha16 source)
        {
            L = source.L;
            A = source.A;
        }

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            L = PixelHelper.ToGray8(source.R, source.G, source.B);
            A = byte.MaxValue;
        }

        public void FromColor(Rgb24 source)
        {
            L = PixelHelper.ToGray8(source.R, source.G, source.B);
            A = byte.MaxValue;
        }

        public void FromColor(Rgb48 source)
        {
            L = PixelHelper.ToGray8(source);
            A = byte.MaxValue;
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            L = PixelHelper.ToGray8(source.R, source.G, source.B);
            A = source.A;
        }

        public void FromColor(Argb32 source)
        {
            L = PixelHelper.ToGray8(source.R, source.G, source.B);
            A = source.A;
        }

        public void FromColor(Bgra32 source)
        {
            L = PixelHelper.ToGray8(source.R, source.G, source.B);
            A = source.A;
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            L = PixelHelper.ToGray8(source.R, source.G, source.B);
            A = source.A;
        }

        public void FromColor(Rgba64 source)
        {
            L = PixelHelper.ToGray8(source);
            A = ScalingHelper.ToUInt8(source.A);
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => A;
        public readonly Alpha16 ToAlpha16() => ScalingHelper.ToUInt16(A);
        public readonly AlphaF ToAlphaF() => ScalingHelper.ToFloat32(A);

        public readonly Gray8 ToGray8() => L;
        public readonly Gray16 ToGray16() => ScalingHelper.ToUInt16(L);
        public readonly GrayF ToGrayF() => ScalingHelper.ToFloat32(L);
        public readonly GrayAlpha16 ToGrayAlpha16() => this;

        public readonly Rgb24 ToRgb24() => new Rgb24(L);
        public readonly Rgb48 ToRgb48() => new Rgb48(ScalingHelper.ToUInt16(L));

        public readonly Color ToRgba32() => new Color(L, A);
        public readonly Rgba64 ToRgba64() => new Rgba64(ScalingHelper.ToUInt16(L), ScalingHelper.ToUInt16(A));

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(ushort other) => PackedValue == other;

        public readonly bool Equals(GrayAlpha16 other) => this == other;

        public static bool operator ==(GrayAlpha16 a, GrayAlpha16 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(GrayAlpha16 a, GrayAlpha16 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is GrayAlpha16 other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        public override readonly string ToString() => nameof(GrayAlpha16) + $"(L:{L}, A:{A})";

        #endregion
    }
}

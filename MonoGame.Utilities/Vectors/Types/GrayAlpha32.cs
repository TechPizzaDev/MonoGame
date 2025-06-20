// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing an 16-bit XYZ luminance and 16-bit W component.
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GrayAlpha32 : IPixel<GrayAlpha32>, IPackedVector<uint>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Luminance),
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Alpha));

        [CLSCompliant(false)]
        public ushort L;

        [CLSCompliant(false)]
        public ushort A;

        [CLSCompliant(false)]
        public GrayAlpha32(ushort luminance, ushort alpha)
        {
            L = luminance;
            A = alpha;
        }

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<GrayAlpha32, uint>(this);
            set => Unsafe.As<GrayAlpha32, uint>(ref this) = value;
        }


        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= ushort.MaxValue;
            scaledVector += new Vector3(0.5f);

            L = (ushort)(PixelHelper.ToGrayF(scaledVector) + 0.5f);
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= ushort.MaxValue;
            scaledVector += new Vector4(0.5f);

            L = (ushort)(PixelHelper.ToGrayF(scaledVector.AsVector3()) + 0.5f);
            A = (ushort)scaledVector.W;
        }

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(L) / ushort.MaxValue;
        public readonly Vector4 ToScaledVector4() => new Vector4(L, L, L, A) / ushort.MaxValue;

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            L = ushort.MaxValue;
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromAlpha(Alpha16 source)
        {
            L = ushort.MaxValue;
            A = source.A;
        }

        public void FromAlpha(Alpha32 source)
        {
            L = ushort.MaxValue;
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromAlpha(AlphaF source)
        {
            L = ushort.MaxValue;
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromGray(Gray8 source)
        {
            L = ScalingHelper.ToUInt16(source.L);
            A = ushort.MaxValue;
        }

        public void FromGray(Gray16 source)
        {
            L = source.L;
            A = ushort.MaxValue;
        }

        public void FromGray(Gray32 source)
        {
            L = ScalingHelper.ToUInt16(source.L);
            A = ushort.MaxValue;
        }

        public void FromGray(GrayF source)
        {
            L = ScalingHelper.ToUInt16(source.L);
            A = ushort.MaxValue;
        }

        public void FromGray(GrayAlpha16 source)
        {
            L = ScalingHelper.ToUInt16(source.L);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            L = PixelHelper.ToGray16(source);
            A = ushort.MaxValue;
        }

        public void FromColor(Rgb24 source)
        {
            L = PixelHelper.ToGray16(source);
            A = ushort.MaxValue;
        }

        public void FromColor(Rgb48 source)
        {
            L = PixelHelper.ToGray16(source);
            A = ushort.MaxValue;
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            L = PixelHelper.ToGray16(source);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Argb32 source)
        {
            L = PixelHelper.ToGray16(source);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Bgra32 source)
        {
            L = PixelHelper.ToGray16(source);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            L = PixelHelper.ToGray16(source);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Rgba64 source)
        {
            L = PixelHelper.ToGray16(source);
            A = source.A;
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => ScalingHelper.ToUInt8(A);
        public readonly Alpha16 ToAlpha16() => A;
        public readonly AlphaF ToAlphaF() => ScalingHelper.ToFloat32(A);

        public readonly Gray8 ToGray8() => ScalingHelper.ToUInt8(L);
        public readonly Gray16 ToGray16() => L;
        public readonly GrayF ToGrayF() => ScalingHelper.ToFloat32(L);
        public readonly GrayAlpha16 ToGrayAlpha16() => new GrayAlpha16(ScalingHelper.ToUInt8(L), ScalingHelper.ToUInt8(A));

        public readonly Rgb24 ToRgb24() => new Rgb24(ScalingHelper.ToUInt8(L));
        public readonly Rgb48 ToRgb48() => new Rgb48(L);

        public readonly Color ToRgba32() => new Color(ScalingHelper.ToUInt8(L), ScalingHelper.ToUInt8(A));
        public readonly Rgba64 ToRgba64() => new Rgba64(L, A);

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(uint other) => PackedValue == other;

        public readonly bool Equals(GrayAlpha32 other) => this == other;

        public static bool operator ==(GrayAlpha32 a, GrayAlpha32 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(GrayAlpha32 a, GrayAlpha32 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is GrayAlpha32 other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        public override readonly string ToString() => nameof(GrayAlpha32) + $"(L:{L}, A:{A})";

        #endregion
    }
}

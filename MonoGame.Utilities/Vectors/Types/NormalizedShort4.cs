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
    /// Packed vector type containing signed 16-bit XYZW components.
    /// <para>
    /// Ranges from [-1, -1, -1, -1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NormalizedShort4 : IPixel<NormalizedShort4>, IPackedVector<ulong>
    {
        internal static Vector3 Offset3 => new Vector3(-short.MinValue);
        internal static Vector4 Offset4 => new Vector4(-short.MinValue);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Int16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.Int16, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.Int16, VectorComponentChannel.Blue),
            new VectorComponent(VectorComponentType.Int16, VectorComponentChannel.Alpha));

        public short X;
        public short Y;
        public short Z;
        public short W;

        #region Constructors

        public NormalizedShort4(short x, short y, short z, short w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        [CLSCompliant(false)]
        public NormalizedShort4(ulong packed) : this()
        {
            // TODO: Unsafe.SkipInit(out this);
            PackedValue = packed;
        }

        #endregion

        #region IPackedVector

        [CLSCompliant(false)]
        public ulong PackedValue
        {
            readonly get => Unsafe.BitCast<NormalizedShort4, ulong>(this);
            set => Unsafe.As<NormalizedShort4, ulong>(ref this) = value;
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= ushort.MaxValue;
            scaledVector -= Offset4;

            X = (short)scaledVector.X;
            Y = (short)scaledVector.Y;
            Z = (short)scaledVector.Z;
            W = (short)scaledVector.W;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= ushort.MaxValue;
            scaledVector -= Offset3;

            X = (short)scaledVector.X;
            Y = (short)scaledVector.Y;
            Z = (short)scaledVector.Z;
        }

        public void FromVector(Vector3 vector)
        {
            vector *= ushort.MaxValue / 2f;
            vector -= new Vector3(0.5f);
            vector = VectorHelper.Clamp(vector, short.MinValue, short.MaxValue);

            X = (short)vector.X;
            Y = (short)vector.Y;
            Z = (short)vector.Z;
            W = short.MaxValue;
        }

        public void FromVector(Vector4 vector)
        {
            vector *= ushort.MaxValue / 2f;
            vector -= new Vector4(0.5f);
            vector = VectorHelper.Clamp(vector, short.MinValue, short.MaxValue);

            X = (short)vector.X;
            Y = (short)vector.Y;
            Z = (short)vector.Z;
            W = (short)vector.W;
        }

        public readonly Vector3 ToScaledVector3()
        {
            var scaled = new Vector3(X, Y, Z);
            scaled += Offset3;
            scaled /= ushort.MaxValue;

            return scaled;
        }

        public readonly Vector4 ToScaledVector4()
        {
            var scaled = new Vector4(X, Y, Z, W);
            scaled += Offset4;
            scaled /= ushort.MaxValue;

            return scaled;
        }

        public readonly Vector3 ToVector3()
        {
            var vector = new Vector3(X, Y, Z);
            vector += Offset3;
            vector *= 2f / ushort.MaxValue;
            vector -= Vector3.One;

            return vector;
        }

        public readonly Vector4 ToVector4()
        {
            var vector = new Vector4(X, Y, Z, W);
            vector += Offset4;
            vector *= 2f / ushort.MaxValue;
            vector -= Vector4.One;

            return vector;
        }

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            X = Y = Z = short.MaxValue;
            W = ScalingHelper.ToInt16(source.A);
        }

        public void FromAlpha(Alpha16 source)
        {
            X = Y = Z = short.MaxValue;
            W = ScalingHelper.ToInt16(source.A);
        }

        public void FromAlpha(Alpha32 source)
        {
            X = Y = Z = short.MaxValue;
            W = ScalingHelper.ToInt16(source.A);
        }

        public void FromAlpha(AlphaF source)
        {
            X = Y = Z = short.MaxValue;
            W = ScalingHelper.ToInt16(source.A);
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

        public readonly Alpha8 ToAlpha8() => ScalingHelper.ToUInt8(W);
        public readonly Alpha16 ToAlpha16() => ScalingHelper.ToUInt16(W);
        public readonly AlphaF ToAlphaF() => ScalingHelper.ToFloat32(W);

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

        public readonly bool Equals(NormalizedShort4 other) => this == other;

        public static bool operator ==(NormalizedShort4 a, NormalizedShort4 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(NormalizedShort4 a, NormalizedShort4 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is NormalizedShort4 other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        public override readonly string ToString() => nameof(NormalizedShort4) + $"({X}, {Y}, {Z}, {W})";

        #endregion
    }
}

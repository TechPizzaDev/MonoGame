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
    /// Packed vector type containing four unsigned 16-bit XYZW components.
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Argb32 : IPixel<Argb32>, IPackedVector<uint>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Alpha),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Blue));

        public byte A;
        public byte R;
        public byte G;
        public byte B;

        /// <summary>
        /// Gets or sets the RGB components of this struct as <see cref="Rgb24"/>
        /// </summary>
        public Rgb24 Rgb
        {
            readonly get => new(R, G, B);
            set
            {
                R = value.R;
                G = value.G;
                B = value.B;
            }
        }

        #region Constructors

        [CLSCompliant(false)]
        public Argb32(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        #endregion

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<Argb32, uint>(this);
            set => Unsafe.As<Argb32, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = ScaledVectorHelper.ToScaledUInt8(scaledVector);
            A = byte.MaxValue;
            R = (byte)scaledVector.X;
            G = (byte)scaledVector.Y;
            B = (byte)scaledVector.Z;
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector = ScaledVectorHelper.ToScaledUInt8(scaledVector);
            A = (byte)scaledVector.W;
            R = (byte)scaledVector.X;
            G = (byte)scaledVector.Y;
            B = (byte)scaledVector.Z;
        }

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(R, G, B) / byte.MaxValue;
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, A) / byte.MaxValue;

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            A = source.A;
            R = G = B = byte.MinValue;
        }

        public void FromAlpha(Alpha16 source)
        {
            A = ScalingHelper.ToUInt8(source.A);
            R = G = B = byte.MinValue;
        }

        public void FromAlpha(Alpha32 source)
        {
            A = ScalingHelper.ToUInt8(source.A);
            R = G = B = byte.MinValue;
        }

        public void FromAlpha(AlphaF source)
        {
            A = ScalingHelper.ToUInt8(source.A);
            R = G = B = byte.MinValue;
        }

        public void FromGray(Gray8 source)
        {
            A = byte.MaxValue;
            R = G = B = source.L;
        }

        public void FromGray(Gray16 source)
        {
            A = byte.MaxValue;
            R = G = B = ScalingHelper.ToUInt8(source.L);
        }

        public void FromGray(Gray32 source)
        {
            A = byte.MaxValue;
            R = G = B = ScalingHelper.ToUInt8(source.L);
        }

        public void FromGray(GrayF source)
        {
            A = byte.MaxValue;
            R = G = B = ScalingHelper.ToUInt8(source.L);
        }

        public void FromGray(GrayAlpha16 source)
        {
            A = source.A;
            R = G = B = source.L;
        }

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            A = byte.MaxValue;
            R = source.R;
            G = source.G;
            B = source.B;
        }

        public void FromColor(Rgb24 source)
        {
            A = byte.MaxValue;
            R = source.R;
            G = source.G;
            B = source.B;
        }

        public void FromColor(Rgb48 source)
        {
            A = byte.MaxValue;
            Rgb = source.ToRgb24();
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            A = source.A;
            R = source.R;
            G = source.G;
            B = source.B;
        }

        public void FromColor(Argb32 source) => this = source;

        public void FromColor(Bgra32 source)
        {
            A = source.A;
            R = source.R;
            G = source.G;
            B = source.B;
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            A = source.A;
            R = source.R;
            G = source.G;
            B = source.B;
        }

        public void FromColor(Rgba64 source)
        {
            A = ScalingHelper.ToUInt8(source.A);
            R = ScalingHelper.ToUInt8(source.R);
            G = ScalingHelper.ToUInt8(source.G);
            B = ScalingHelper.ToUInt8(source.B);
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => A;
        public readonly Alpha16 ToAlpha16() => ScalingHelper.ToUInt16(A);
        public readonly AlphaF ToAlphaF() => ScalingHelper.ToFloat32(A);

        public readonly Gray8 ToGray8() => PixelHelper.ToGray8(R, G, B);
        public readonly Gray16 ToGray16() => PixelHelper.ToGray16(this);
        public readonly GrayF ToGrayF() => PixelHelper.ToGrayF(this);
        public readonly GrayAlpha16 ToGrayAlpha16() => PixelHelper.ToGrayAlpha16(R, G, B, A);

        public readonly Rgb24 ToRgb24() => new Rgb24(R, G, B);

        public readonly Rgb48 ToRgb48()
        {
            return new Rgb48(
                ScalingHelper.ToUInt16(R),
                ScalingHelper.ToUInt16(G),
                ScalingHelper.ToUInt16(B));
        }

        public readonly Color ToRgba32() => new Color(R, G, B, A);

        public readonly Rgba64 ToRgba64()
        {
            return new Rgba64(
                ScalingHelper.ToUInt16(R),
                ScalingHelper.ToUInt16(G),
                ScalingHelper.ToUInt16(B),
                ScalingHelper.ToUInt16(A));
        }

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(uint other) => PackedValue == other;

        public readonly bool Equals(Argb32 other) => this == other;

        public static bool operator ==(Argb32 a, Argb32 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(Argb32 a, Argb32 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Argb32 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Argb32) + $"(R:{R}, G:{G}, B:{B}, A:{A})";

        #endregion
    }
}
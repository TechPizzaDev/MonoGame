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
    public struct Bgra32 : IPixel<Bgra32>, IPackedVector<uint>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Blue),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Alpha));

        public byte B;
        public byte G;
        public byte R;
        public byte A;

        /// <summary>
        /// Gets or sets the RGB components of this struct as <see cref="Rgb24"/>
        /// </summary>
        public Bgr24 Bgr
        {
            readonly get => Unsafe.BitCast<Bgra32, Bgr24>(this);
            set => Unsafe.As<Bgra32, Bgr24>(ref this) = value;
        }

        #region Constructors

        [CLSCompliant(false)]
        public Bgra32(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        [CLSCompliant(false)]
        public Bgra32(uint packedValue) : this()
        {
            PackedValue = packedValue;
        }

        #endregion

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<Bgra32, uint>(this);
            set => Unsafe.As<Bgra32, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = ScaledVectorHelper.ToScaledUInt8(scaledVector);
            B = (byte)scaledVector.Z;
            G = (byte)scaledVector.Y;
            R = (byte)scaledVector.X;
            A = byte.MaxValue;
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= byte.MaxValue;
            scaledVector += new Vector4(0.5f);

            B = (byte)scaledVector.Z;
            G = (byte)scaledVector.Y;
            R = (byte)scaledVector.X;
            A = (byte)scaledVector.W;
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
            B = G = R = byte.MinValue;
            A = source.A;
        }

        public void FromAlpha(Alpha16 source)
        {
            B = G = R = byte.MinValue;
            A = ScalingHelper.ToUInt8(source.A);
        }

        public void FromAlpha(Alpha32 source)
        {
            B = G = R = byte.MinValue;
            A = ScalingHelper.ToUInt8(source.A);
        }

        public void FromAlpha(AlphaF source)
        {
            B = G = R = byte.MinValue;
            A = ScalingHelper.ToUInt8(source.A);
        }

        public void FromGray(Gray8 source)
        {
            B = G = R = source.L;
            A = byte.MaxValue;
        }

        public void FromGray(Gray16 source)
        {
            B = G = R = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(Gray32 source)
        {
            B = G = R = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(GrayF source)
        {
            B = G = R = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(GrayAlpha16 source)
        {
            B = G = R = source.L;
            A = source.A;
        }

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            Bgr = source;
            A = byte.MaxValue;
        }

        public void FromColor(Rgb24 source)
        {
            B = source.B;
            G = source.G;
            R = source.R;
            A = byte.MaxValue;
        }

        public void FromColor(Rgb48 source)
        {
            B = ScalingHelper.ToUInt8(source.B);
            G = ScalingHelper.ToUInt8(source.G);
            R = ScalingHelper.ToUInt8(source.R);
            A = byte.MaxValue;
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            B = source.B;
            G = source.G;
            R = source.R;
            A = source.A;
        }

        public void FromColor(Argb32 source)
        {
            B = source.B;
            G = source.G;
            R = source.R;
            A = source.A;
        }

        public void FromColor(Bgra32 source) => this = source;

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            B = source.B;
            G = source.G;
            R = source.R;
            A = source.A;
        }

        public void FromColor(Rgba64 source)
        {
            B = ScalingHelper.ToUInt8(source.B);
            G = ScalingHelper.ToUInt8(source.G);
            R = ScalingHelper.ToUInt8(source.R);
            A = ScalingHelper.ToUInt8(source.A);
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

        public readonly Bgr24 ToBgr24() => Bgr;
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

        public readonly bool Equals(Bgra32 other) => this == other;

        public static bool operator ==(Bgra32 a, Bgra32 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(Bgra32 a, Bgra32 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Bgra32 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Bgra32) + $"(R:{R}, G:{G}, B:{B}, A:{A})";

        #endregion
    }
}
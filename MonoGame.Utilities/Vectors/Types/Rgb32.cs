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
    /// Packed vector type containing three unsigned 8-bit XYZ components.
    /// Padded with an extra 8 bits.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rgb32 : IPixel<Rgb32>
    {
        VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Blue),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Padding));

        public byte R;
        public byte G;
        public byte B;
        public byte Padding;

        /// <summary>
        /// Gets or sets the RGB components of this <see cref="Rgb32"/> as <see cref="Rgb24"/>
        /// </summary>
        public Rgb24 Rgb
        {
            readonly get => Unsafe.BitCast<Rgb32, Rgb24>(this);
            set => Unsafe.As<Rgb32, Rgb24>(ref this) = value;
        }

        #region Constructors

        public Rgb32(byte r, byte g, byte b, byte padding = default)
        {
            R = r;
            G = g;
            B = b;
            Padding = padding;
        }

        #endregion

        #region IPackedVector

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = ScaledVectorHelper.ToScaledUInt8(scaledVector);
            R = (byte)scaledVector.X;
            G = (byte)scaledVector.Y;
            B = (byte)scaledVector.Z;
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(R, G, B) / byte.MaxValue;
        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => R = G = B = byte.MaxValue;
        public void FromAlpha(Alpha16 source) => R = G = B = byte.MaxValue;
        public void FromAlpha(Alpha32 source) => R = G = B = byte.MaxValue;
        public void FromAlpha(AlphaF source) => R = G = B = byte.MaxValue;

        public void FromGray(Gray8 source) => R = G = B = source.L;
        public void FromGray(Gray16 source) => R = G = B = ScalingHelper.ToUInt8(source.L);
        public void FromGray(Gray32 source) => R = G = B = ScalingHelper.ToUInt8(source.L);
        public void FromGray(GrayF source) => R = G = B = ScalingHelper.ToUInt8(source.L);
        public void FromGray(GrayAlpha16 source) => R = G = B = source.L;

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
        }

        public void FromColor(Rgb24 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
        }

        public void FromColor(Rgb48 source)
        {
            R = ScalingHelper.ToUInt8(source.R);
            G = ScalingHelper.ToUInt8(source.G);
            B = ScalingHelper.ToUInt8(source.B);
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Argb32 source)
        {
            B = source.B;
            G = source.G;
            R = source.R;
        }

        public void FromColor(Abgr32 source)
        {
            B = source.B;
            G = source.G;
            R = source.R;
        }

        public void FromColor(Bgra32 source)
        {
            B = source.B;
            G = source.G;
            R = source.R;
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            B = source.B;
            G = source.G;
            R = source.R;
        }

        public void FromColor(Rgba64 source)
        {
            R = ScalingHelper.ToUInt8(source.R);
            G = ScalingHelper.ToUInt8(source.G);
            B = ScalingHelper.ToUInt8(source.B);
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => Alpha8.Opaque;
        public readonly Alpha16 ToAlpha16() => Alpha16.Opaque;
        public readonly AlphaF ToAlphaF() => AlphaF.Opaque;

        public readonly Gray8 ToGray8() => PixelHelper.ToGray8(R, G, B);
        public readonly Gray16 ToGray16() => PixelHelper.ToGray16(this);
        public readonly GrayF ToGrayF() => PixelHelper.ToGrayF(this);
        public readonly GrayAlpha16 ToGrayAlpha16() => PixelHelper.ToGrayAlpha16(R, G, B);

        public readonly Rgb24 ToRgb24() => new Rgb24(R, G, B);

        public readonly Rgb48 ToRgb48()
        {
            return new Rgb48(
                ScalingHelper.ToUInt16(R),
                ScalingHelper.ToUInt16(G),
                ScalingHelper.ToUInt16(B));
        }

        public readonly Color ToRgba32() => new Color(R, G, B);

        public readonly Rgba64 ToRgba64()
        {
            return new Rgba64(
                ScalingHelper.ToUInt16(R),
                ScalingHelper.ToUInt16(G),
                ScalingHelper.ToUInt16(B));
        }

        #endregion

        #region Equals

        public readonly bool Equals(Rgb32 other) => this == other;

        // We don't want to compare padding.
        public static bool operator ==(Rgb32 a, Rgb32 b) => a.R == b.R && a.G == b.G && a.B == b.B;
        public static bool operator !=(Rgb32 a, Rgb32 b) => !(a == b);

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Rgb32 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(R, G, B);

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Rgb32) + $"(R:{R}, G:{G}, B:{B})";

        #endregion

        public static implicit operator Rgb24(in Rgb32 value) => value.Rgb;
    }
}
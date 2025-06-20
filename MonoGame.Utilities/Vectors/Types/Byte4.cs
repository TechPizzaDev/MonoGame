// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing unsigned 8-bit XYZW integer components.
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [255, 255, 255, 255] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Byte4 : IPixel<Byte4>, IPackedVector<uint>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Blue),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Alpha));

        public byte X;
        public byte Y;
        public byte Z;
        public byte W;

        public Rgb24 Rgb
        {
            readonly get => Unsafe.BitCast<Byte4, Rgb24>(this);
            set => Unsafe.As<Byte4, Rgb24>(ref this) = value;
        }

        public Color Rgba
        {
            readonly get => Unsafe.BitCast<Byte4, Color>(this);
            set => Unsafe.As<Byte4, Color>(ref this) = value;
        }

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with a packed value.
        /// </summary>
        [CLSCompliant(false)]
        public Byte4(uint packed) : this()
        {
            // TODO: Unsafe.SkipInit(out this)
            PackedValue = packed;
        }

        /// <summary>
        /// Constructs the packed vector with raw values.
        /// </summary>
        public Byte4(byte x, byte y, byte z, byte w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        #endregion

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<Byte4, uint>(this);
            set => Unsafe.As<Byte4, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= byte.MaxValue;
            scaledVector += new Vector3(0.5f);

            X = (byte)scaledVector.X;
            Y = (byte)scaledVector.Y;
            Z = (byte)scaledVector.Z;
            W = byte.MaxValue;
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= byte.MaxValue;
            scaledVector += new Vector4(0.5f);

            X = (byte)scaledVector.X;
            Y = (byte)scaledVector.Y;
            Z = (byte)scaledVector.Z;
            W = (byte)scaledVector.W;
        }

        public void FromVector(Vector3 vector)
        {
            vector = VectorHelper.ScaledClamp(vector);
            vector += new Vector3(0.5f);

            X = (byte)vector.X;
            Y = (byte)vector.Y;
            Z = (byte)vector.Z;
            W = byte.MaxValue;
        }

        public void FromVector(Vector4 vector)
        {
            vector = VectorHelper.ScaledClamp(vector);
            vector += new Vector4(0.5f);

            X = (byte)vector.X;
            Y = (byte)vector.Y;
            Z = (byte)vector.Z;
            W = (byte)vector.W;
        }

        public readonly Vector3 ToScaledVector3() => ToVector3() / byte.MaxValue;
        public readonly Vector4 ToScaledVector4() => ToVector4() / byte.MaxValue;

        public readonly Vector3 ToVector3() => new Vector3(X, Y, Z);
        public readonly Vector4 ToVector4() => new Vector4(X, Y, Z, W);

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            X = Y = Z = byte.MaxValue;
            W = source.A;
        }

        public void FromAlpha(Alpha16 source)
        {
            X = Y = Z = byte.MaxValue;
            W = ScalingHelper.ToUInt8(source.A);
        }

        public void FromAlpha(Alpha32 source)
        {
            X = Y = Z = byte.MaxValue;
            W = ScalingHelper.ToUInt8(source.A);
        }

        public void FromAlpha(AlphaF source)
        {
            X = Y = Z = byte.MaxValue;
            W = ScalingHelper.ToUInt8(source.A);
        }

        public void FromGray(Gray8 source)
        {
            X = Y = Z = source.L;
            W = byte.MaxValue;
        }

        public void FromGray(Gray16 source)
        {
            X = Y = Z = ScalingHelper.ToUInt8(source.L);
            W = byte.MaxValue;
        }

        public void FromGray(Gray32 source)
        {
            X = Y = Z = ScalingHelper.ToUInt8(source.L);
            W = byte.MaxValue;
        }

        public void FromGray(GrayF source)
        {
            X = Y = Z = ScalingHelper.ToUInt8(source.L);
            W = byte.MaxValue;
        }

        public void FromGray(GrayAlpha16 source)
        {
            X = Y = Z = source.L;
            W = source.A;
        }

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            X = source.R;
            Y = source.G;
            Z = source.B;
            W = byte.MaxValue;
        }

        public void FromColor(Rgb24 source)
        {
            X = source.R;
            Y = source.G;
            Z = source.B;
            W = byte.MaxValue;
        }

        public void FromColor(Rgb48 source)
        {
            X = ScalingHelper.ToUInt8(source.R);
            Y = ScalingHelper.ToUInt8(source.G);
            Z = ScalingHelper.ToUInt8(source.B);
            W = byte.MaxValue;
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            X = source.R;
            Y = source.G;
            Z = source.B;
            W = source.A;
        }

        public void FromColor(Argb32 source)
        {
            X = source.R;
            Y = source.G;
            Z = source.B;
            W = source.A;
        }

        public void FromColor(Bgra32 source)
        {
            X = source.R;
            Y = source.G;
            Z = source.B;
            W = source.A;
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source) => Rgba = source;

        public void FromColor(Rgba64 source)
        {
            X = ScalingHelper.ToUInt8(source.R);
            Y = ScalingHelper.ToUInt8(source.G);
            Z = ScalingHelper.ToUInt8(source.B);
            W = ScalingHelper.ToUInt8(source.A);
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => W;
        public readonly Alpha16 ToAlpha16() => ScalingHelper.ToUInt16(W);
        public readonly AlphaF ToAlphaF() => ScalingHelper.ToFloat32(W);

        public readonly Gray8 ToGray8() => PixelHelper.ToGray8(X, Y, Z);
        public readonly Gray16 ToGray16() => PixelHelper.ToGray16(this);
        public readonly GrayF ToGrayF() => PixelHelper.ToGrayF(this);
        public readonly GrayAlpha16 ToGrayAlpha16() => PixelHelper.ToGrayAlpha16(X, Y, Z, W);

        public readonly Bgr24 ToBgr24() => new Bgr24(X, Y, Z);
        public readonly Rgb24 ToRgb24() => Rgb;

        public readonly Rgb48 ToRgb48()
        {
            return new Rgb48(
                ScalingHelper.ToUInt16(X),
                ScalingHelper.ToUInt16(Y),
                ScalingHelper.ToUInt16(Z));
        }

        public readonly Color ToRgba32() => Rgba;

        public readonly Rgba64 ToRgba64()
        {
            return new Rgba64(
                ScalingHelper.ToUInt16(X),
                ScalingHelper.ToUInt16(Y),
                ScalingHelper.ToUInt16(Z),
                ScalingHelper.ToUInt16(W));
        }

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(uint other) => PackedValue == other;

        public readonly bool Equals(Byte4 other) => this == other;

        public static bool operator ==(Byte4 a, Byte4 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(Byte4 a, Byte4 b) => a.PackedValue != b.PackedValue;

        #endregion

        /// <summary>
        /// Gets the hexadecimal <see cref="string"/> representation of this <see cref="Byte4"/>.
        /// </summary>
        public readonly string ToHex(IFormatProvider? provider) => PackedValue.ToString("x8", provider);

        /// <summary>
        /// Gets the hexadecimal <see cref="string"/> representation of this <see cref="Byte4"/>.
        /// </summary>
        public readonly string ToHex() => ToHex(CultureInfo.CurrentCulture);

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Byte4 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Returns a <see cref="string"/> representation of this packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Byte4) + $"({ToVector4()}";

        #endregion
    }
}


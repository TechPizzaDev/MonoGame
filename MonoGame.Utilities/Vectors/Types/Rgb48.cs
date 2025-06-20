// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing three unsigned 16-bit XYZ components.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rgb48 : IPixel<Rgb48>
    {
        public static Rgb48 Black => new Rgb48(ushort.MinValue, ushort.MinValue, ushort.MinValue);
        public static Rgb48 White => new Rgb48(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Blue));

        [CLSCompliant(false)]
        public ushort R;

        [CLSCompliant(false)]
        public ushort G;

        [CLSCompliant(false)]
        public ushort B;

        #region Constructors

        [CLSCompliant(false)]
        public Rgb48(ushort r, ushort g, ushort b)
        {
            R = r;
            G = g;
            B = b;
        }

        [CLSCompliant(false)]
        public Rgb48(ushort value) : this(value, value, value)
        {
        }

        #endregion

        #region IPackedVector

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= ushort.MaxValue;
            scaledVector += new Vector3(0.5f);

            R = (ushort)scaledVector.X;
            G = (ushort)scaledVector.Y;
            B = (ushort)scaledVector.Z;
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(R, G, B) / ushort.MaxValue;
        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => R = G = B = byte.MaxValue;
        public void FromAlpha(Alpha16 source) => R = G = B = byte.MaxValue;
        public void FromAlpha(Alpha32 source) => R = G = B = byte.MaxValue;
        public void FromAlpha(AlphaF source) => R = G = B = byte.MaxValue;

        public void FromGray(Gray8 source) => R = G = B = ScalingHelper.ToUInt16(source.L);
        public void FromGray(Gray16 source) => R = G = B = source.L;
        public void FromGray(Gray32 source) => R = G = B = ScalingHelper.ToUInt16(source.L);
        public void FromGray(GrayF source) => R = G = B = ScalingHelper.ToUInt16(source.L);
        public void FromGray(GrayAlpha16 source) => R = G = B = ScalingHelper.ToUInt16(source.L);

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
        }

        public void FromColor(Rgb24 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
        }

        public void FromColor(Rgb48 source) => this = source;

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
        }

        public void FromColor(Argb32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
        }

        public void FromColor(Bgra32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
        }

        public void FromColor(Rgba64 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => Alpha8.Opaque;
        public readonly Alpha16 ToAlpha16() => Alpha16.Opaque;
        public readonly AlphaF ToAlphaF() => AlphaF.Opaque;

        public readonly Gray8 ToGray8() => PixelHelper.ToGray8(this);
        public readonly Gray16 ToGray16() => PixelHelper.ToGray16(R, G, B);
        public readonly GrayF ToGrayF() => PixelHelper.ToGrayF(this);
        public readonly GrayAlpha16 ToGrayAlpha16() => PixelHelper.ToGrayAlpha16(this);

        public readonly Rgb24 ToRgb24()
        {
            return new Rgb24(
                ScalingHelper.ToUInt8(R),
                ScalingHelper.ToUInt8(G),
                ScalingHelper.ToUInt8(B));
        }

        public readonly Rgb48 ToRgb48() => this;

        public readonly Color ToRgba32()
        {
            return new Color(
                ScalingHelper.ToUInt8(R),
                ScalingHelper.ToUInt8(G),
                ScalingHelper.ToUInt8(B));
        }

        public readonly Rgba64 ToRgba64() => new Rgba64(R, G, B);

        #endregion

        #region Equals

        public readonly bool Equals(Rgb48 other) => this == other;

        public static bool operator ==(Rgb48 a, Rgb48 b) => a.R == b.R && a.G == b.G && a.B == b.B;
        public static bool operator !=(Rgb48 a, Rgb48 b) => !(a == b);

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Rgb48 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(R, G, B);

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Rgb48) + $"(R:{R}, G:{G}, B:{B})";

        #endregion
    }
}
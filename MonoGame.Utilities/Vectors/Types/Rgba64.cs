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
    public struct Rgba64 : IPixel<Rgba64>, IPackedVector<ulong>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Blue),
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Alpha));

        [CLSCompliant(false)]
        public ushort R;

        [CLSCompliant(false)]
        public ushort G;

        [CLSCompliant(false)]
        public ushort B;

        [CLSCompliant(false)]
        public ushort A;

        /// <summary>
        /// Gets or sets the RGB components of this struct as <see cref="Rgb48"/>
        /// </summary>
        public Rgb48 Rgb
        {
            readonly get => Unsafe.BitCast<Rgba64, Rgb48>(this);
            set => Unsafe.As<Rgba64, Rgb48>(ref this) = value;
        }

        #region Constructors

        [CLSCompliant(false)]
        public Rgba64(ushort r, ushort g, ushort b, ushort a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        [CLSCompliant(false)]
        public Rgba64(ushort r, ushort g, ushort b) : this(r, g, b, ushort.MaxValue)
        {
        }

        [CLSCompliant(false)]
        public Rgba64(ushort luminance, ushort alpha) : this(luminance, luminance, luminance, alpha)
        {
        }

        #endregion

        #region IPackedVector

        [CLSCompliant(false)]
        public ulong PackedValue
        {
            readonly get => Unsafe.BitCast<Rgba64, ulong>(this);
            set => Unsafe.As<Rgba64, ulong>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= ushort.MaxValue;
            scaledVector += new Vector3(0.5f);

            R = (ushort)scaledVector.X;
            G = (ushort)scaledVector.Y;
            B = (ushort)scaledVector.Z;
            A = ushort.MaxValue;
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= ushort.MaxValue;
            scaledVector += new Vector4(0.5f);

            R = (ushort)scaledVector.X;
            G = (ushort)scaledVector.Y;
            B = (ushort)scaledVector.Z;
            A = (ushort)scaledVector.W;
        }

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(R, G, B) / ushort.MaxValue;
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, A) / ushort.MaxValue;

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            R = G = B = ushort.MaxValue;
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromAlpha(Alpha16 source)
        {
            R = G = B = ushort.MaxValue;
            A = source.A;
        }

        public void FromAlpha(Alpha32 source)
        {
            R = G = B = ushort.MaxValue;
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromAlpha(AlphaF source)
        {
            R = G = B = ushort.MaxValue;
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromGray(Gray8 source)
        {
            R = G = B = ScalingHelper.ToUInt16(source.L);
            A = ushort.MaxValue;
        }

        public void FromGray(Gray16 source)
        {
            R = G = B = source.L;
            A = ushort.MaxValue;
        }

        public void FromGray(Gray32 source)
        {
            R = G = B = ScalingHelper.ToUInt16(source.L);
            A = ushort.MaxValue;
        }

        public void FromGray(GrayF source)
        {
            R = G = B = ScalingHelper.ToUInt16(source.L);
            A = ushort.MaxValue;
        }

        public void FromGray(GrayAlpha16 source)
        {
            R = G = B = ScalingHelper.ToUInt16(source.L);
            A = source.A;
        }

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
            A = ushort.MaxValue;
        }

        public void FromColor(Rgb24 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
            A = ushort.MaxValue;
        }

        public void FromColor(Rgb48 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
            A = ushort.MaxValue;
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Argb32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Bgra32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
            B = ScalingHelper.ToUInt16(source.B);
            A = ScalingHelper.ToUInt16(source.A);
        }

        public void FromColor(Rgba64 source)
        {
            this = source;
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => ScalingHelper.ToUInt8(A);
        public readonly Alpha16 ToAlpha16() => A;
        public readonly AlphaF ToAlphaF() => ScalingHelper.ToFloat32(A);

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

        public readonly Rgb48 ToRgb48() => Rgb;

        public readonly Color ToRgba32()
        {
            return new Color(
                ScalingHelper.ToUInt8(R),
                ScalingHelper.ToUInt8(G),
                ScalingHelper.ToUInt8(B),
                ScalingHelper.ToUInt8(A));
        }

        public readonly Rgba64 ToRgba64() => this;

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(ulong other) => PackedValue == other;

        public readonly bool Equals(Rgba64 other) => this == other;

        public static bool operator ==(Rgba64 a, Rgba64 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(Rgba64 a, Rgba64 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Rgba64 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Rgba64) + $"(R:{R}, G:{G}, B:{B}, A:{A})";

        #endregion
    }
}
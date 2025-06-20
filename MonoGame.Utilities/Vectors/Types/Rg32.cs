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
    /// Packed vector type containing signed 16-bit XY components.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 0, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rg32 : IPixel<Rg32>, IPackedVector<uint>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt16, VectorComponentChannel.Green));

        [CLSCompliant(false)]
        public ushort R;

        [CLSCompliant(false)]
        public ushort G;

        #region Constructors

        [CLSCompliant(false)]
        public Rg32(ushort x, ushort y)
        {
            R = x;
            G = y;
        }

        [CLSCompliant(false)]
        public Rg32(uint packed) : this()
        {
            // TODO: Unsafe.SkipInit(out this);
            PackedValue = packed;
        }

        #endregion

        /// <summary>
        /// Gets the packed vector in <see cref="Vector2"/> format.
        /// </summary>
        public readonly Vector2 ToScaledVector2() => new Vector2(R, G) / ushort.MaxValue;

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<Rg32, uint>(this);
            set => Unsafe.As<Rg32, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            var vector = scaledVector.AsVector2();
            vector = VectorHelper.ScaledClamp(vector);
            vector *= ushort.MaxValue;
            vector += new Vector2(0.5f);

            R = (ushort)vector.X;
            G = (ushort)vector.Y;
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(ToScaledVector2(), 0);
        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => R = G = ushort.MaxValue;
        public void FromAlpha(Alpha16 source) => R = G = ushort.MaxValue;
        public void FromAlpha(Alpha32 source) => R = G = ushort.MaxValue;
        public void FromAlpha(AlphaF source) => R = G = ushort.MaxValue;

        public void FromGray(Gray8 source) => R = G = ScalingHelper.ToUInt16(source.L);
        public void FromGray(Gray16 source) => R = G = source.L;
        public void FromGray(Gray32 source) => R = G = ScalingHelper.ToUInt16(source.L);
        public void FromGray(GrayF source) => R = G = ScalingHelper.ToUInt16(source.L);
        public void FromGray(GrayAlpha16 source) => R = G = ScalingHelper.ToUInt16(source.L);

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgr24 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
        }

        public void FromColor(Rgb24 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
        }

        public void FromColor(Rgb48 source)
        {
            R = source.R;
            G = source.G;
        }

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Abgr32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
        }

        public void FromColor(Argb32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
        }

        public void FromColor(Bgra32 source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source)
        {
            R = ScalingHelper.ToUInt16(source.R);
            G = ScalingHelper.ToUInt16(source.G);
        }

        public void FromColor(Rgba64 source)
        {
            R = source.R;
            G = source.G;
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => Alpha8.Opaque;
        public readonly Alpha16 ToAlpha16() => Alpha16.Opaque;
        public readonly AlphaF ToAlphaF() => AlphaF.Opaque;

        public readonly Gray8 ToGray8() => PixelHelper.ToGray8(this);
        public readonly Gray16 ToGray16() => PixelHelper.ToGray16(R, G, 0);
        public readonly GrayF ToGrayF() => PixelHelper.ToGrayF(this);
        public readonly GrayAlpha16 ToGrayAlpha16() => PixelHelper.ToGrayAlpha16(this);

        public readonly Rgb24 ToRgb24() => ToRgb48().ToRgb24();
        public readonly Rgb48 ToRgb48() => new Rgb48(R, G, 0);

        public readonly Color ToRgba32() => ToRgba64().ToRgba32();
        public readonly Rgba64 ToRgba64() => new Rgba64(R, G, 0);

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(uint other) => PackedValue == other;

        public readonly bool Equals(Rg32 other) => this == other;

        public static bool operator ==(Rg32 a, Rg32 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(Rg32 a, Rg32 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Rg32 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Rg32) + $"(R:{R}, G:{G})";

        #endregion
    }
}

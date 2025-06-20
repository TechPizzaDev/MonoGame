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
    // TODO: make use of Net5 "Half" type 

    /// <summary>
    /// Packed vector type containing a 16-bit floating-point X component.
    /// <para>Ranges from [-1, 0, 0, 1] to [1, 0, 0, 1] in vector form.</para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HalfSingle : IPixel<HalfSingle>, IPackedVector<ushort>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Float16, VectorComponentChannel.Red));

        public static HalfSingle Zero => default;
        public static HalfSingle One { get; } = new HalfSingle(1);

        private ushort _packed;

        #region Constructors

        /// <summary>
        /// Constructs the vector with a packed value.
        /// </summary>
        [CLSCompliant(false)]
        public HalfSingle(ushort packed) : this()
        {
            _packed = packed;
        }

        /// <summary>
        /// Constructs the vector with a vector form value.
        /// </summary>
        public HalfSingle(float single) : this(HalfTypeHelper.Pack(single))
        {
        }

        #endregion

        public static HalfSingle FromScaled(float scaledSingle) => new HalfSingle(scaledSingle * 2f - 1f);

        /// <summary>
        /// Gets the vector as a <see cref="float"/>.
        /// </summary>
        public readonly float ToSingle() => HalfTypeHelper.Unpack(PackedValue);

        /// <summary>
        /// Gets the packed vector as a <see cref="float"/>.
        /// </summary>
        public readonly float ToScaledSingle() => (ToSingle() + 1) / 2f;

        #region IPackedVector

        [CLSCompliant(false)]
        public ushort PackedValue
        {
            readonly get => Unsafe.BitCast<HalfSingle, ushort>(this);
            set => Unsafe.As<HalfSingle, ushort>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            float scaled = scaledVector.X;
            scaled *= 2;
            scaled -= 1;

            PackedValue = HalfTypeHelper.Pack(scaled);
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(ToScaledSingle(), 0, 0);
        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => this = One;
        public void FromAlpha(Alpha16 source) => this = One;
        public void FromAlpha(Alpha32 source) => this = One;
        public void FromAlpha(AlphaF source) => this = One;

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

        public Alpha8 ToAlpha8() => Alpha8.Opaque;
        public Alpha16 ToAlpha16() => Alpha16.Opaque;
        public AlphaF ToAlphaF() => AlphaF.Opaque;

        public Gray8 ToGray8() => ScalingHelper.ToUInt8(ToScaledSingle() * PixelHelper.GrayFactor.X);
        public Gray16 ToGray16() => ScalingHelper.ToUInt16(ToScaledSingle() * PixelHelper.GrayFactor.X);
        public GrayF ToGrayF() => ToScaledSingle() * PixelHelper.GrayFactor.X;
        public GrayAlpha16 ToGrayAlpha16() => new GrayAlpha16(ToGray8(), byte.MaxValue);

        public readonly Rgb24 ToRgb24() => new Rgb24(ScalingHelper.ToUInt8(ToScaledSingle()), 0, 0);
        public readonly Rgb48 ToRgb48() => new Rgb48(ScalingHelper.ToUInt16(ToScaledSingle()), 0, 0);

        public readonly Color ToRgba32() => Color.FromBytes(ScalingHelper.ToUInt8(ToScaledSingle()), 0, 0);
        public readonly Rgba64 ToRgba64() => new Rgba64(ScalingHelper.ToUInt16(ToScaledSingle()), 0, 0);

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(ushort other) => PackedValue == other;

        public readonly bool Equals(HalfSingle other) => this == other;

        public static bool operator ==(HalfSingle a, HalfSingle b) => a._packed == b._packed;
        public static bool operator !=(HalfSingle a, HalfSingle b) => a._packed != b._packed;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is HalfSingle other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        public readonly string ToString(IFormatProvider? provider) => ToSingle().ToString(provider);

        /// <summary>
        /// Gets a string representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => ToString(CultureInfo.CurrentCulture);

        #endregion

        public static explicit operator HalfSingle(float value) => new HalfSingle(value);
        public static implicit operator float(HalfSingle value) => value.ToSingle();
    }
}

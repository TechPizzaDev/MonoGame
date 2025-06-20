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
    /// Packed vector type containing an 32-bit W component.
    /// <para>
    /// Ranges from [1, 1, 1, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AlphaF : IPixel<AlphaF>, IPackedVector<uint>
    {
        public static AlphaF Transparent => default;
        public static AlphaF Opaque => new AlphaF(1f);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Float32, VectorComponentChannel.Alpha));

        [CLSCompliant(false)]
        public float A;

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with a raw value.
        /// </summary>
        /// <param name="alpha">The W component.</param>
        public AlphaF(float alpha)
        {
            A = alpha;
        }

        /// <summary>
        /// Constructs the packed vector with a packed value.
        /// </summary>
        [CLSCompliant(false)]
        public AlphaF(uint value) : this()
        {
            // TODO: Unsafe.SkipInit(out this)
            PackedValue = value;
        }

        #endregion

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<AlphaF, uint>(this);
            set => Unsafe.As<AlphaF, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector) => A = 1f;
        public void FromScaledVector(Vector4 scaledVector) => A = scaledVector.W;

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => Vector3.One;
        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), A);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => A = ScalingHelper.ToFloat32(source.A);
        public void FromAlpha(Alpha16 source) => A = ScalingHelper.ToFloat32(source.A);
        public void FromAlpha(Alpha32 source) => A = ScalingHelper.ToFloat32(source.A);
        public void FromAlpha(AlphaF source) => this = source;

        public void FromGray(Gray8 source) => A = 1f;
        public void FromGray(Gray16 source) => A = 1f;
        public void FromGray(Gray32 source) => A = 1f;
        public void FromGray(GrayF source) => A = 1f;
        public void FromGray(GrayAlpha16 source) => A = ScalingHelper.ToFloat32(source.A);

        public void FromColor(Bgr565 source) => A = 1f;
        public void FromColor(Bgr24 source) => A = 1f;
        public void FromColor(Rgb24 source) => A = 1f;
        public void FromColor(Rgb48 source) => A = 1f;

        public void FromColor(Bgra4444 source) => this = source.ToAlphaF();
        public void FromColor(Bgra5551 source) => this = source.ToAlphaF();
        public void FromColor(Abgr32 source) => A = ScalingHelper.ToFloat32(source.A);
        public void FromColor(Argb32 source) => A = ScalingHelper.ToFloat32(source.A);
        public void FromColor(Bgra32 source) => A = ScalingHelper.ToFloat32(source.A);
        public void FromColor(Rgba1010102 source) => this = source.ToAlphaF();
        public void FromColor(Color source) => A = ScalingHelper.ToFloat32(source.A);
        public void FromColor(Rgba64 source) => A = ScalingHelper.ToFloat32(source.A);

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => ScalingHelper.ToUInt8(A);

        public readonly Alpha16 ToAlpha16() => ScalingHelper.ToUInt16(A);

        public readonly AlphaF ToAlphaF() => this;

        public readonly Gray8 ToGray8() => Gray8.White;

        public readonly Gray16 ToGray16() => Gray16.White;

        public readonly GrayAlpha16 ToGrayAlpha16() => GrayAlpha16.OpaqueWhite;

        public readonly GrayF ToGrayF() => GrayF.White;

        public readonly Rgb24 ToRgb24() => Rgb24.White;

        public readonly Color ToRgba32() => new Color(byte.MaxValue, ScalingHelper.ToUInt8(A));

        public readonly Rgb48 ToRgb48() => Rgb48.White;

        public readonly Rgba64 ToRgba64() => new Rgba64(ushort.MaxValue, ScalingHelper.ToUInt16(A));

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(uint other) => PackedValue == other;

        public readonly bool Equals(AlphaF other) => this == other;

        public static bool operator ==(AlphaF a, AlphaF b) => a.A == b.A;
        public static bool operator !=(AlphaF a, AlphaF b) => a.A != b.A;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is AlphaF other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(A);

        /// <summary>
        /// Gets a string representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(AlphaF) + $"({A})";

        #endregion

        public static implicit operator AlphaF(float alpha) => new AlphaF(alpha);
        public static implicit operator float(AlphaF value) => value.A;
    }
}

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
    /// Packed vector type containing two signed 16-bit integers.
    /// <para>
    /// Ranges from [-32768, -32768, 0, 1] to [32767, 32767, 0, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Short2 : IPixel<Short2>, IPackedVector<uint>
    {
        private static Vector2 Offset2 => new Vector2(32768);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Int16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.Int16, VectorComponentChannel.Green));

        public short X;
        public short Y;

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with raw values.
        /// </summary>
        [CLSCompliant(false)]
        public Short2(short x, short y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructs the packed vector with a packed value.
        /// </summary>
        [CLSCompliant(false)]
        public Short2(uint packed) : this()
        {
            // TODO: Unsafe.SkipInit(out this);
            PackedValue = packed;
        }

        #endregion

        public readonly Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Pack(Vector2 vector, out Short2 destination)
        {
            vector = VectorHelper.Clamp(vector, short.MinValue, short.MaxValue);
            vector = VectorHelper.Round(vector);

            destination.X = (short)vector.X;
            destination.Y = (short)vector.Y;
        }

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<Short2, uint>(this);
            set => Unsafe.As<Short2, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            var vector = scaledVector.AsVector2();
            vector *= ushort.MaxValue;
            vector -= Offset2;

            Pack(vector, out this);
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => Pack(vector.AsVector2(), out this);
        public void FromVector(Vector4 vector) => FromVector(vector.AsVector3());

        public readonly Vector3 ToScaledVector3()
        {
            var scaledVector = ToVector2();
            scaledVector += Offset2;
            scaledVector /= ushort.MaxValue;

            return new Vector3(scaledVector, 0);
        }

        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => new Vector3(ToVector2(), 0);
        public readonly Vector4 ToVector4() => new Vector4(ToVector3(), 1);

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => X = Y = short.MaxValue;
        public void FromAlpha(Alpha16 source) => X = Y = short.MaxValue;
        public void FromAlpha(Alpha32 source) => X = Y = short.MaxValue;
        public void FromAlpha(AlphaF source) => X = Y = short.MaxValue;

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

        public readonly Alpha8 ToAlpha8() => Alpha8.Opaque;
        public readonly Alpha16 ToAlpha16() => Alpha16.Opaque;
        public readonly AlphaF ToAlphaF() => AlphaF.Opaque;

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
        public readonly bool Equals(uint other) => PackedValue == other;

        public readonly bool Equals(Short2 other) => this == other;

        public static bool operator ==(Short2 a, Short2 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(Short2 a, Short2 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Short2 other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        public override readonly string ToString() => nameof(Short2) + $"({X}, {Y})";

        #endregion
    }
}
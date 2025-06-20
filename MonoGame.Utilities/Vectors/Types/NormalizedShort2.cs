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
    /// Ranges from [-1, -1, 0, 1] to [1, 1, 0, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NormalizedShort2 : IPixel<NormalizedShort2>, IPackedVector<uint>
    {
        internal static Vector2 Offset => new Vector2(-short.MinValue);

        public static NormalizedShort2 MaxValue => new NormalizedShort2(short.MaxValue, short.MaxValue);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Int16, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.Int16, VectorComponentChannel.Green));

        public short X;
        public short Y;

        #region Constructors

        public NormalizedShort2(short x, short y)
        {
            X = x;
            Y = y;
        }

        [CLSCompliant(false)]
        public NormalizedShort2(uint packed) : this()
        {
            // TODO: Unsafe.SkipInit(out this);
            PackedValue = packed;
        }

        #endregion

        public readonly Vector2 ToVector2()
        {
            var vector = new Vector2(X, Y);
            vector += Offset;
            vector /= 2f * ushort.MaxValue;
            vector -= Vector2.One;

            return vector;
        }

        #region IPackedVector

        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<NormalizedShort2, uint>(this);
            set => Unsafe.As<NormalizedShort2, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            var raw = scaledVector.AsVector2();
            raw = VectorHelper.ScaledClamp(raw);
            raw *= ushort.MaxValue;
            raw -= Offset;

            X = (short)raw.X;
            Y = (short)raw.Y;
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector)
        {
            var raw = vector.AsVector2();
            raw = Vector2.Clamp(raw, -Vector2.One, Vector2.One);
            raw *= ushort.MaxValue / 2f;
            raw = VectorHelper.Round(raw);
            
            X = (short)raw.X;
            Y = (short)raw.Y;
        }

        public void FromVector(Vector4 vector) => FromVector(vector.AsVector3());

        public readonly Vector3 ToScaledVector3()
        {
            var scaled = new Vector2(X, Y);
            scaled += Offset;
            scaled /= ushort.MaxValue;

            return new Vector3(scaled, 0f);
        }

        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1f);

        public readonly Vector3 ToVector3() => new Vector3(ToVector2(), 0f);
        public readonly Vector4 ToVector4() => new Vector4(ToVector3(), 1f);

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => this = MaxValue;
        public void FromAlpha(Alpha16 source) => this = MaxValue;
        public void FromAlpha(Alpha32 source) => this = MaxValue;
        public void FromAlpha(AlphaF source) => this = MaxValue;

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

        public readonly bool Equals(NormalizedShort2 other) => this == other;

        public static bool operator ==(NormalizedShort2 a, NormalizedShort2 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(NormalizedShort2 a, NormalizedShort2 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is NormalizedShort2 other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        public override readonly string ToString() => nameof(NormalizedShort2) + $"({X}, {Y})";

        #endregion
    }
}

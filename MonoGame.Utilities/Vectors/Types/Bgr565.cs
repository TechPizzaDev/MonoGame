// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing unsigned XYZ components.
    /// The XZ components use 5 bits each, and the Y component uses 6 bits.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Bgr565 : IPixel<Bgr565>, IPackedVector<ushort>
    {
        public const int MaxXZ = 0x1F;
        public const int MaxY = 0x3F;

        private static Vector3 MaxValue3 => new Vector3(MaxXZ, MaxY, MaxXZ);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.BitField, VectorComponentChannel.Blue, 5),
            new VectorComponent(VectorComponentType.BitField, VectorComponentChannel.Green, 6),
            new VectorComponent(VectorComponentType.BitField, VectorComponentChannel.Red, 5));

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with a packed value.
        /// </summary>
        /// <param name="alpha">The alpha component.</param>
        [CLSCompliant(false)]
        public Bgr565(ushort packed)
        {
            PackedValue = packed;
        }

        /// <summary>
        /// Constructs the packed vector with vector form values.
        /// </summary>
        /// <param name="scaledVector"><see cref="Vector3"/> containing the components.</param>
        public Bgr565(Vector3 scaledVector) : this()
        {
            // TODO: Unsafe.SkipInit(out this)
            FromScaledVector(scaledVector);
        }

        /// <summary>
        /// Constructs the packed vector with vector form values.
        /// </summary>
        public Bgr565(float r, float g, float b) : this(new Vector3(r, g, b))
        {
        }

        #endregion

        #region IPackedVector

        [CLSCompliant(false)]
        public ushort PackedValue { get; set; }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= MaxValue3;
            scaledVector += new Vector3(0.5f);

            PackedValue = (ushort)(
                ((int)scaledVector.Z & MaxXZ) |
                (((int)scaledVector.Y & MaxY) << 5) |
                (((int)scaledVector.X & MaxXZ) << 11));
        }

        public void FromScaledVector(Vector4 scaledVector) => FromScaledVector(scaledVector.AsVector3());

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3()
        {
            return new Vector3(
                (PackedValue >> 11) & MaxXZ,
                (PackedValue >> 5) & MaxY,
                PackedValue & MaxXZ) / MaxValue3;
        }

        public readonly Vector4 ToScaledVector4() => new Vector4(ToScaledVector3(), 1);

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source) => PackedValue = ushort.MaxValue;
        public void FromAlpha(Alpha16 source) => PackedValue = ushort.MaxValue;
        public void FromAlpha(Alpha32 source) => PackedValue = ushort.MaxValue;
        public void FromAlpha(AlphaF source) => PackedValue = ushort.MaxValue;

        public void FromGray(Gray8 source) => FromColor(source.ToRgb24());
        public void FromGray(Gray16 source) => FromColor(source.ToRgb48());
        public void FromGray(Gray32 source) => FromScaledVector(source.ToScaledVector3());
        public void FromGray(GrayF source) => FromScaledVector(source.ToScaledVector3());
        public void FromGray(GrayAlpha16 source) => FromColor(source.ToRgba32());

        public void FromColor(Bgr565 source) => this = source;
        public void FromColor(Bgr24 source) => FromColor(source.ToRgb24());
        public void FromColor(Rgb24 source) => FromScaledVector(source.ToScaledVector3());
        public void FromColor(Rgb48 source) => FromScaledVector(source.ToScaledVector3());

        public void FromColor(Bgra4444 source)
        {
            ushort packedSource = source.PackedValue;
            PackedValue = (ushort)(
                ((packedSource & Bgra4444.MaxXYZW) * 2) |
                ((((packedSource >> 4) & Bgra4444.MaxXYZW) * 4) << 5) |
                ((((packedSource >> 8) & Bgra4444.MaxXYZW) * 2) << 11));
        }

        public void FromColor(Bgra5551 source)
        {
            ushort packedSource = source.PackedValue;
            PackedValue = (ushort)(
                (packedSource & Bgra5551.MaxXYZ) |
                ((((packedSource >> 5) & Bgra5551.MaxXYZ) * 2) << 5) |
                (((packedSource >> 10) & Bgra5551.MaxXYZ) << 11));
        }

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
        public readonly bool Equals(ushort other) => PackedValue == other;

        public readonly bool Equals(Bgr565 other) => this == other;

        public static bool operator ==(Bgr565 a, Bgr565 b) => a.PackedValue == b.PackedValue;
        public static bool operator !=(Bgr565 a, Bgr565 b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is Bgr565 other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Gets a string representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(Bgr565) + $"({ToScaledVector3()})";

        #endregion
    }
}

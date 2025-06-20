using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing four 32-bit floating-point XYZ components.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RgbVector : IPixel<RgbVector>
    {
        public static RgbVector One => new RgbVector(Vector3.One);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Float32, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.Float32, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.Float32, VectorComponentChannel.Blue));

        public Vector3 Base;

        public float R { readonly get => Base.X; set => Base.X = value; }
        public float G { readonly get => Base.Y; set => Base.Y = value; }
        public float B { readonly get => Base.Z; set => Base.Z = value; }

        public RgVector Rg
        {
            readonly get => Unsafe.BitCast<RgbVector, RgVector>(this);
            set => Unsafe.As<RgbVector, RgVector>(ref this) = value;
        }

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with a vector.
        /// </summary>
        public RgbVector(Vector3 vector)
        {
            Base = vector;
        }

        /// <summary>
        /// Constructs the packed vector with vector form values.
        /// </summary>
        public RgbVector(float r, float g, float b) : this(new Vector3(r, g, b))
        {
        }

        #endregion

        #region IPackedVector

        public void FromScaledVector(Vector3 scaledVector) => Base = scaledVector;
        public void FromScaledVector(Vector4 scaledVector) => Base = scaledVector.AsVector3();

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => Base;
        public readonly Vector4 ToScaledVector4() => Base.ToVector4();

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

        public readonly bool Equals(RgbVector other) => Base.Equals(other.Base);

        public static bool operator ==(RgbVector a, RgbVector b) => a.Base == b.Base;
        public static bool operator !=(RgbVector a, RgbVector b) => a.Base != b.Base;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is RgbVector other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => Base.GetHashCode();

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(RgbVector) + $"({Base})";

        #endregion

        public static implicit operator RgbVector(Vector3 vector) => Unsafe.BitCast<Vector3, RgbVector>(vector);
        public static implicit operator Vector3(RgbVector vector) => Unsafe.BitCast<RgbVector, Vector3>(vector);
    }
}

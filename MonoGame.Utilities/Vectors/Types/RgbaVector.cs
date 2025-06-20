using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Packed vector type containing four 32-bit floating-point XYZW components.
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RgbaVector : IPixel<RgbaVector>
    {
        public static RgbaVector One => new RgbaVector(Vector4.One);

        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.Float32, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.Float32, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.Float32, VectorComponentChannel.Blue),
            new VectorComponent(VectorComponentType.Float32, VectorComponentChannel.Alpha));

        public Vector4 Base;

        public float R { readonly get => Base.X; set => Base.X = value; }
        public float G { readonly get => Base.Y; set => Base.Y = value; }
        public float B { readonly get => Base.Z; set => Base.Z = value; }
        public float A { readonly get => Base.W; set => Base.W = value; }

        public RgVector Rg
        {
            readonly get => Unsafe.BitCast<RgbaVector, RgVector>(this);
            set => Unsafe.As<RgbaVector, RgVector>(ref this) = value;
        }

        public RgbVector Rgb
        {
            readonly get => Unsafe.BitCast<RgbaVector, RgbVector>(this);
            set => Unsafe.As<RgbaVector, RgbVector>(ref this) = value;
        }

        #region Constructors

        /// <summary>
        /// Constructs the packed vector with a vector.
        /// </summary>
        public RgbaVector(Vector4 vector)
        {
            Base = vector;
        }

        /// <summary>
        /// Constructs the packed vector with vector form values.
        /// </summary>
        public RgbaVector(float r, float g, float b, float a) : this(new Vector4(r, g, b, a))
        {
        }

        #endregion

        #region IPackedVector

        public void FromScaledVector(Vector3 scaledVector) => Base = scaledVector.ToVector4();
        public void FromScaledVector(Vector4 scaledVector) => Base = scaledVector;

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => Base.AsVector3();
        public readonly Vector4 ToScaledVector4() => Base;

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            R = G = B = 1f;
            A = ScalingHelper.ToFloat32(source.A);
        }

        public void FromAlpha(Alpha16 source)
        {
            R = G = B = 1f;
            A = ScalingHelper.ToFloat32(source.A);
        }

        public void FromAlpha(Alpha32 source)
        {
            R = G = B = 1f;
            A = ScalingHelper.ToFloat32(source.A);
        }

        public void FromAlpha(AlphaF source)
        {
            R = G = B = 1f;
            A = source.A;
        }

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

        public readonly Alpha8 ToAlpha8() => ScalingHelper.ToUInt8(A);
        public readonly Alpha16 ToAlpha16() => ScalingHelper.ToUInt16(A);
        public readonly AlphaF ToAlphaF() => A;

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

        public readonly bool Equals(RgbaVector other) => Base.Equals(other.Base);

        public static bool operator ==( RgbaVector a, RgbaVector b) => a.Base == b.Base;
        public static bool operator !=( RgbaVector a, RgbaVector b) => a.Base != b.Base;

        #endregion

        #region Object overrides

        public override readonly bool Equals(object? obj) => obj is RgbaVector other && Equals(other);

        /// <summary>
        /// Gets a hash code of the packed vector.
        /// </summary>
        public override readonly int GetHashCode() => Base.GetHashCode();

        /// <summary>
        /// Gets a <see cref="string"/> representation of the packed vector.
        /// </summary>
        public override readonly string ToString() => nameof(RgbaVector) + $"({Base})";

        #endregion

        public static implicit operator RgbaVector(Vector4 vector) => Unsafe.BitCast<Vector4, RgbaVector>(vector);
        public static implicit operator Vector4(RgbaVector vector) => Unsafe.BitCast<RgbaVector, Vector4>(vector);
    }
}

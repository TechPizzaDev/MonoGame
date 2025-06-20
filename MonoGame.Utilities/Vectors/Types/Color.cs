// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using MonoGame.Framework.Vectors;

namespace MonoGame.Framework
{
    /// <summary>
    /// Packed vector type containing unsigned 8-bit RGBA components.
    /// <para>Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.</para>
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Color : IPixel<Color>, IPackedVector<uint>
    {
        readonly VectorComponentInfo IVector.ComponentInfo => new VectorComponentInfo(
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Red),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Green),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Blue),
            new VectorComponent(VectorComponentType.UInt8, VectorComponentChannel.Alpha));

        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        [DataMember]
        public byte R;

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        [DataMember]
        public byte G;

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        [DataMember]
        public byte B;

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        [DataMember]
        public byte A;

        #region Properties

        /// <summary>
        /// Gets or sets the RGB components of this <see cref="Color"/> as <see cref="Rgb24"/>
        /// </summary>
        public Rgb24 Rgb
        {
            readonly get => Unsafe.BitCast<Color, Rgb24>(this);
            set => Unsafe.As<Color, Rgb24>(ref this) = value;
        }

        /// <summary>
        /// Gets or sets the RGB components of this struct as <see cref="Bgr24"/> reverting the component order.
        /// </summary>
        public Bgr24 Bgr
        {
            readonly get => new Bgr24(R, G, B);
            set
            {
                R = value.R;
                G = value.G;
                B = value.B;
            }
        }

        internal string DebuggerDisplay => ToString();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the <see cref="Color"/> with raw values.
        /// </summary>
        /// <remarks>
        /// This overload sets the values directly without clamping and 
        /// may therefore be faster than other overloads.
        /// </remarks>
        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> with raw values.
        /// </summary>
        /// <remarks>
        /// This overload sets the values directly without clamping and 
        /// may therefore be faster than other overloads.
        /// </remarks>
        public Color(byte r, byte g, byte b) : this(r, g, b, byte.MaxValue)
        {
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> with raw values.
        /// </summary>
        /// <remarks>
        /// This overload sets the values directly without clamping and 
        /// may therefore be faster than other overloads.
        /// </remarks>
        public Color(byte luminance, byte alpha) : this(luminance, luminance, luminance, alpha)
        {
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> with a packed value.
        /// R in the least significant byte.
        /// </summary>
        /// <remarks>
        /// This overload sets the values directly without clamping and 
        /// may therefore be faster than other overloads.
        /// </remarks>
        [CLSCompliant(false)]
        public Color(uint packed) : this()
        {
            // TODO: Unsafe.SkipInit(out this)
            PackedValue = packed;
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> with clamped raw values. 
        /// <para>
        /// The values are clamped between <see cref="byte.MinValue"/> and <see cref="byte.MaxValue"/>.
        /// </para>
        /// </summary>
        /// <param name="r">Red component value from 0 to 255.</param>
        /// <param name="g">Green component value from 0 to 255.</param>
        /// <param name="b">Blue component value from 0 to 255.</param>
        /// <param name="a">Alpha component value from 0 to 255.</param>
        public Color(int r, int g, int b, int a) : this(
            (byte)Math.Clamp(r, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(g, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(b, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(a, byte.MinValue, byte.MaxValue))
        {
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> with clamped raw values. 
        /// Alpha value will be opaque.
        /// <para>
        /// The values are clamped between <see cref="byte.MinValue"/> and <see cref="byte.MaxValue"/>.
        /// </para>
        /// </summary>
        /// <param name="r">Red component value from 0 to 255.</param>
        /// <param name="g">Green component value from 0 to 255.</param>
        /// <param name="b">Blue component value from 0 to 255.</param>
        public Color(int r, int g, int b) : this(
            (byte)Math.Clamp(r, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(g, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(b, byte.MinValue, byte.MaxValue))
        {
        }

        #region Constructors (float and Vector)

        /// <summary>
        /// Constructs the <see cref="Color"/> with vector form values.
        /// </summary>
        /// <param name="scaledVector"><see cref="Vector4"/> containing the components.</param>
        public Color(Vector4 scaledVector) : this()
        {
            // TODO: Unsafe.SkipInit(out this)
            FromScaledVector(scaledVector);
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> from vector form values.
        /// </summary>
        /// <param name="r">Red component value from 0 to 1.</param>
        /// <param name="g">Green component value from 0 to 1.</param>
        /// <param name="b">Blue component value from 0 to 1.</param>
        /// <param name="a">Alpha component value from 0 to 1.</param>
        public Color(float r, float g, float b, float a) : this(new Vector4(r, g, b, a))
        {
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> with vector form values.
        /// Alpha value will be opaque.
        /// <para>The values are clamped between 0 and 1.</para>
        /// </summary>
        /// <param name="color"><see cref="Vector3"/> containing the components.</param>
        public Color(Vector3 color)
        {
            color = VectorHelper.ScaledClamp(color);
            color *= byte.MaxValue;
            
            R = (byte)color.X;
            G = (byte)color.Y;
            B = (byte)color.Z;
            A = byte.MaxValue;
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> with vector form values. 
        /// Alpha value will be opaque.
        /// </summary>
        /// <param name="r">Red component value from 0 to 1.</param>
        /// <param name="g">Green component value from 0 to 1.</param>
        /// <param name="b">Blue component value from 0 to 1.</param>
        public Color(float r, float g, float b) : this(new Vector3(r, g, b))
        {
        }

        #endregion

        #region Constructors (Color)

        /// <summary>
        /// Constructs <see cref="Color"/> from a <see cref="Color"/> and alpha value.
        /// </summary>
        /// <param name="color">The RGB values.</param>
        /// <param name="alpha">The alpha component value from 0 to 255.</param>
        public Color(Color color, byte alpha) : this(color.R, color.G, color.B, alpha)
        {
        }

        /// <summary>
        /// Constructs <see cref="Color"/> from an <see cref="Color"/> and alpha value.
        /// </summary>
        /// <param name="color">The RGB values.</param>
        /// <param name="alpha">Alpha component value from 0 to 255.</param>
        public Color(Color color, int alpha) :
            this(color, (byte)Math.Clamp(alpha, byte.MinValue, byte.MaxValue))
        {
        }

        /// <summary>
        /// Constructs the <see cref="Color"/> from a <see cref="Color"/> and alpha value.
        /// </summary>
        /// <param name="color">The RGB values.</param>
        /// <param name="alpha">Alpha component value from 0 to 1.</param>
        public Color(Color color, float alpha) :
            this(color, MathHelper.ClampTruncate(alpha * byte.MaxValue, byte.MinValue, byte.MaxValue))
        {
        }

        #endregion

        #region Constructors (Rgb24)

        /// <summary>
        /// Constructs <see cref="Color"/> from an <see cref="Rgb24"/> and alpha value.
        /// </summary>
        /// <param name="color">The RGB values.</param>
        public Color(Rgb24 color) :
            this(color.R, color.G, color.B, byte.MaxValue)
        {
        }

        /// <summary>
        /// Constructs <see cref="Color"/> from an <see cref="Rgb24"/> and alpha value.
        /// </summary>
        /// <param name="color">The RGB values.</param>
        /// <param name="alpha">The alpha component value from 0 to 255.</param>
        public Color(Rgb24 color, byte alpha) :
            this(color.R, color.G, color.B, alpha)
        {
        }

        /// <summary>
        /// Constructs <see cref="Color"/> from an <see cref="Rgb24"/> and alpha value.
        /// </summary>
        /// <param name="color">The RGB values.</param>
        /// <param name="alpha">Alpha component value from 0 to 1.</param>
        public Color(Rgb24 color, int alpha) :
            this(color, (byte)Math.Clamp(alpha, byte.MinValue, byte.MaxValue))
        {
        }

        /// <summary>
        /// Constructs <see cref="Color"/> from an <see cref="Rgb24"/> and alpha value.
        /// </summary>
        /// <param name="color">The RGB values.</param>
        /// <param name="alpha">Alpha component value from 0 to 1.</param>
        public Color(Rgb24 color, float alpha) :
            this(color, MathHelper.ClampTruncate(alpha * byte.MaxValue, byte.MinValue, byte.MaxValue))
        {
        }

        #endregion

        #endregion

        #region IPackedVector

        [IgnoreDataMember]
        [CLSCompliant(false)]
        public uint PackedValue
        {
            readonly get => Unsafe.BitCast<Color, uint>(this);
            set => Unsafe.As<Color, uint>(ref this) = value;
        }

        public void FromScaledVector(Vector3 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= byte.MaxValue;
            scaledVector += new Vector3(0.5f);

            R = (byte)scaledVector.X;
            G = (byte)scaledVector.Y;
            B = (byte)scaledVector.Z;
            A = byte.MaxValue;
        }

        public void FromScaledVector(Vector4 scaledVector)
        {
            scaledVector = VectorHelper.ScaledClamp(scaledVector);
            scaledVector *= byte.MaxValue;
            scaledVector += new Vector4(0.5f);

            R = (byte)scaledVector.X;
            G = (byte)scaledVector.Y;
            B = (byte)scaledVector.Z;
            A = (byte)scaledVector.W;
        }

        public void FromVector(Vector3 vector) => FromScaledVector(vector);
        public void FromVector(Vector4 vector) => FromScaledVector(vector);

        public readonly Vector3 ToScaledVector3() => new Vector3(R, G, B) / byte.MaxValue;
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, A) / byte.MaxValue;

        public readonly Vector3 ToVector3() => ToScaledVector3();
        public readonly Vector4 ToVector4() => ToScaledVector4();

        #endregion

        #region IPixel.From

        public void FromAlpha(Alpha8 source)
        {
            R = G = B = byte.MaxValue;
            A = source;
        }

        public void FromAlpha(Alpha16 source)
        {
            R = G = B = byte.MaxValue;
            A = ScalingHelper.ToUInt8(source);
        }

        public void FromAlpha(Alpha32 source)
        {
            R = G = B = byte.MaxValue;
            A = ScalingHelper.ToUInt8(source.A);
        }

        public void FromAlpha(AlphaF source)
        {
            R = G = B = byte.MaxValue;
            A = ScalingHelper.ToUInt8(source);
        }

        public void FromGray(Gray8 source)
        {
            R = G = B = source.L;
            A = byte.MaxValue;
        }

        public void FromGray(Gray16 source)
        {
            R = G = B = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(Gray32 source)
        {
            R = G = B = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(GrayF source)
        {
            R = G = B = ScalingHelper.ToUInt8(source.L);
            A = byte.MaxValue;
        }

        public void FromGray(GrayAlpha16 source)
        {
            R = G = B = source.L;
            A = source.A;
        }

        public void FromColor(Bgr565 source) => FromColor(source.ToRgb24());

        public void FromColor(Bgra4444 source) => FromColor(source.ToRgba32());
        public void FromColor(Bgra5551 source) => FromColor(source.ToRgba32());

        public void FromColor(Bgr24 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
            A = byte.MaxValue;
        }

        public void FromColor(Rgb24 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
            A = byte.MaxValue;
        }

        public void FromColor(Rgb48 source)
        {
            R = ScalingHelper.ToUInt8(source.R);
            G = ScalingHelper.ToUInt8(source.G);
            B = ScalingHelper.ToUInt8(source.B);
            A = byte.MaxValue;
        }

        public void FromColor(Abgr32 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
            A = source.A;
        }

        public void FromColor(Argb32 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
            A = source.A;
        }

        public void FromColor(Bgra32 source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
            A = source.A;
        }

        public void FromColor(Rgba1010102 source) => FromScaledVector(source.ToScaledVector4());

        public void FromColor(Color source) => this = source;

        public void FromColor(Rgba64 source)
        {
            R = ScalingHelper.ToUInt8(source.R);
            G = ScalingHelper.ToUInt8(source.G);
            B = ScalingHelper.ToUInt8(source.B);
            A = ScalingHelper.ToUInt8(source.A);
        }

        #endregion

        #region IPixel.To

        public readonly Alpha8 ToAlpha8() => A;
        public readonly Alpha16 ToAlpha16() => ScalingHelper.ToUInt16(A);
        public readonly AlphaF ToAlphaF() => ScalingHelper.ToFloat32(A);

        public readonly Gray8 ToGray8() => PixelHelper.ToGray8(R, G, B);
        public readonly Gray16 ToGray16() => PixelHelper.ToGray16(this);
        public readonly GrayF ToGrayF() => PixelHelper.ToGrayF(this);
        public readonly GrayAlpha16 ToGrayAlpha16() => PixelHelper.ToGrayAlpha16(R, G, B, A);

        public readonly Bgr24 ToBgr24() => new Bgr24(R, G, B);
        public readonly Rgb24 ToRgb24() => Rgb;

        public readonly Rgb48 ToRgb48()
        {
            return new Rgb48(
                ScalingHelper.ToUInt16(R),
                ScalingHelper.ToUInt16(G),
                ScalingHelper.ToUInt16(B));
        }

        public readonly Abgr32 ToAbgr32() => new Abgr32(R, G, B, A);
        public readonly Argb32 ToArgb32() => new Argb32(R, G, B, A);
        public readonly Bgra32 ToBgra32() => new Bgra32(R, G, B, A);
        public readonly Color ToRgba32() => this;

        public readonly Rgba64 ToRgba64()
        {
            return new Rgba64(
                ScalingHelper.ToUInt16(R),
                ScalingHelper.ToUInt16(G),
                ScalingHelper.ToUInt16(B),
                ScalingHelper.ToUInt16(A));
        }

        #endregion

        /// <summary>
        /// Used to hint that the <see cref="Color(byte, byte, byte, byte)"/> constructor should be used.
        /// </summary>
        public static Color FromBytes(byte r, byte g, byte b, byte a = 255) => new Color(r, g, b, a);

        public static Color FromBytes(ReadOnlySpan<byte> source) => MemoryMarshal.Read<Color>(source);

        /// <summary>
        /// Translate a non-premultipled alpha <see cref="Vector4"/> color to a
        /// <see cref="Color"/> that contains premultiplied alpha.
        /// </summary>
        /// <param name="vector">A <see cref="Vector4"/> representing color.</param>
        /// <returns>A <see cref="Color"/> which contains premultiplied alpha data.</returns>
        public static Color FromNonPremultiplied(Vector4 vector)
        {
            float w = vector.W;
            vector.W = 1;
            vector *= w;
            return new Color(vector);
        }

        /// <summary>
        /// Translate a non-premultipled alpha color to a <see cref="Color"/> that contains premultiplied alpha.
        /// </summary>
        /// <param name="r">Red component value.</param>
        /// <param name="g">Green component value.</param>
        /// <param name="b">Blue component value.</param>
        /// <param name="a">Alpha component value.</param>
        /// <returns>A <see cref="Color"/> which contains premultiplied alpha data.</returns>
        public static Color FromNonPremultiplied(int r, int g, int b, int a)
        {
            int aFac = a / 255;
            return new Color(r * aFac, g * aFac, b * aFac, a);
        }

        /// <summary>
        /// Performs linear interpolation of <see cref="Color"/>.
        /// </summary>
        /// <param name="start">Source <see cref="Color"/>.</param>
        /// <param name="end">Destination <see cref="Color"/>.</param>
        /// <param name="amount">Interpolation factor.</param>
        /// <returns>Interpolated <see cref="Color"/>.</returns>
        public static Color Lerp(Color start, Color end, float amount)
        {
            amount = Math.Clamp(amount, 0, 1);
            
            return new Color(
                (byte)MathHelper.Lerp(start.R, end.R, amount),
                (byte)MathHelper.Lerp(start.G, end.G, amount),
                (byte)MathHelper.Lerp(start.B, end.B, amount),
                (byte)MathHelper.Lerp(start.A, end.A, amount));
        }

        /// <summary>
        /// Multiply <see cref="Color"/> by value.
        /// </summary>
        /// <param name="value">Source <see cref="Color"/>.</param>
        /// <param name="scale">Factor.</param>
        /// <returns>Multiplication result.</returns>
        public static Color Multiply(Color value, float scale) => value * scale;

        /// <summary>
        /// Multiply <see cref="Color"/> by a scalar.
        /// </summary>
        /// <param name="value">Source <see cref="Color"/>.</param>
        /// <param name="scale">Factor.</param>
        /// <returns>Multiplication result.</returns>
        public static Color operator *(Color value, float scale)
        {
            return new Color(
                (int)(value.R * scale),
                (int)(value.G * scale),
                (int)(value.B * scale),
                (int)(value.A * scale));
        }

        /// <summary>
        /// Gets the hexadecimal <see cref="string"/> representation of this <see cref="Color"/>.
        /// </summary>
        public readonly string ToHex(IFormatProvider? provider) => PackedValue.ToString("x8", provider);

        /// <summary>
        /// Gets the hexadecimal <see cref="string"/> representation of this <see cref="Color"/>.
        /// </summary>
        public readonly string ToHex() => ToHex(CultureInfo.CurrentCulture);

        #region Deconstruct

        /// <summary>
        /// Deconstruction method for <see cref="Color"/>.
        /// </summary>
        /// <param name="r">Red component value from 0 to 255.</param>
        /// <param name="g">Green component value from 0 to 255.</param>
        /// <param name="b">Blue component value from 0 to 255.</param>
        public readonly void Deconstruct(out byte r, out byte g, out byte b)
        {
            r = R;
            g = G;
            b = B;
        }

        /// <summary>
        /// Deconstruction method for <see cref="Color"/>.
        /// </summary>
        /// <param name="r">Red component value from 0.0f to 1.0f.</param>
        /// <param name="g">Green component value from 0.0f to 1.0f.</param>
        /// <param name="b">Blue component value from 0.0f to 1.0f.</param>
        public readonly void Deconstruct(out float r, out float g, out float b)
        {
            var vector = ToScaledVector3();
            r = vector.X;
            g = vector.Y;
            b = vector.Z;
        }

        /// <summary>
        /// Deconstruction method for <see cref="Color"/> with Alpha.
        /// </summary>
        /// <param name="r">Red component value from 0 to 255.</param>
        /// <param name="g">Green component value from 0 to 255.</param>
        /// <param name="b">Blue component value from 0 to 255.</param>
        /// <param name="a">Alpha component value from 0 to 255.</param>
        public readonly void Deconstruct(out byte r, out byte g, out byte b, out byte a)
        {
            r = R;
            g = G;
            b = B;
            a = A;
        }

        /// <summary>
        /// Deconstruction method for <see cref="Color"/> with Alpha.
        /// </summary>
        /// <param name="r">Red component value from 0.0f to 1.0f.</param>
        /// <param name="g">Green component value from 0.0f to 1.0f.</param>
        /// <param name="b">Blue component value from 0.0f to 1.0f.</param>
        /// <param name="a">Alpha component value from 0.0f to 1.0f.</param>
        public readonly void Deconstruct(out float r, out float g, out float b, out float a)
        {
            var vector = ToScaledVector4();
            r = vector.X;
            g = vector.Y;
            b = vector.Z;
            a = vector.W;
        }

        #endregion

        #region Equals

        [CLSCompliant(false)]
        public readonly bool Equals(uint other) => PackedValue == other;

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Color"/>.
        /// </summary>
        public readonly bool Equals(Color other) => this == other;

        /// <summary>
        /// Compares whether two <see cref="Color"/> instances are equal.
        /// </summary>
        public static bool operator ==(Color a, Color b) => a.PackedValue == b.PackedValue;

        /// <summary>
        /// Compares whether two <see cref="Color"/> instances are not equal.
        /// </summary>
        public static bool operator !=(Color a, Color b) => a.PackedValue != b.PackedValue;

        #endregion

        #region Object overrides

        /// <summary>
        /// Compares whether current instance is equal to specified object.
        /// </summary>
        public override readonly bool Equals(object? obj) => obj is Color other && Equals(other);

        /// <summary>
        /// Gets a hash code of the <see cref="Color"/>.
        /// </summary>
        public override readonly int GetHashCode() => HashCode.Combine(PackedValue);

        /// <summary>
        /// Returns a <see cref="string"/> representation of this <see cref="Color"/>.
        /// </summary>
        public override readonly string ToString()
        {
            var sb = new StringBuilder(28);
            sb.Append("(R:");
            sb.Append(R);
            sb.Append(", G:");
            sb.Append(G);
            sb.Append(", B:");
            sb.Append(B);
            sb.Append(", A:");
            sb.Append(A);
            sb.Append(')');
            return sb.ToString();
        }

        #endregion
    }
}

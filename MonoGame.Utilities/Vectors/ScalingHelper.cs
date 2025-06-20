using System;
using System.Runtime.CompilerServices;

namespace MonoGame.Framework.Vectors
{
    public static class ScalingHelper
    {
        private const MethodImplOptions ImplOptions = 
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;

        private const float UInt1Max = 0b1;
        private const float UInt2Max = 0b11;
        private const float UInt4Max = 0b1111;
        private const float UInt5Max = 0b1_1111;
        private const float UInt6Max = 0b11_1111;

        // TODO: optimize some of these

        #region "Error catchers"

        private const string ErrorMessage = 
            "This method does nothing with the passed value. Did you mean to use the value directly?";

        [CLSCompliant(false)]
        [Obsolete(ErrorMessage)]
        [MethodImpl(ImplOptions)]
        public static sbyte ToInt8(sbyte value)
        {
            return value;
        }

        [Obsolete(ErrorMessage)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt8(byte value)
        {
            return value;
        }

        [Obsolete(ErrorMessage)]
        [MethodImpl(ImplOptions)]
        public static short ToInt16(short value)
        {
            return value;
        }

        [CLSCompliant(false)]
        [Obsolete(ErrorMessage)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16(ushort value)
        {
            return value;
        }

        [Obsolete(ErrorMessage)]
        [MethodImpl(ImplOptions)]
        public static int ToInt32(int value)
        {
            return value;
        }

        [CLSCompliant(false)]
        [Obsolete(ErrorMessage)]
        [MethodImpl(ImplOptions)]
        public static uint ToUInt32(uint value)
        {
            return value;
        }

        [Obsolete(ErrorMessage)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32(float value)
        {
            return value;
        }

        [Obsolete(ErrorMessage)]
        [MethodImpl(ImplOptions)]
        public static double ToFloat64(double value)
        {
            return value;
        }

        #endregion

        #region Integer to Float scaling

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32FromUInt1(byte value)
        {
            return value / UInt1Max;
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32FromUInt2(byte value)
        {
            return value / UInt2Max;
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32FromUInt4(byte value)
        {
            return value / UInt4Max;
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32FromUInt5(byte value)
        {
            return value / UInt5Max;
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32FromUInt6(byte value)
        {
            return value / UInt6Max;
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32(sbyte value)
        {
            return ToFloat32(ToUInt8(value));
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32(byte value)
        {
            return value / (float)byte.MaxValue;
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32(ushort value)
        {
            return value / (float)ushort.MaxValue;
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32(short value)
        {
            return ToFloat32(ToUInt16(value));
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static double ToFloat64(uint value)
        {
            return value / (double)uint.MaxValue;
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static float ToFloat32(uint value)
        {
            return (float)ToFloat64(value);
        }

        #endregion

        #region Float to Integer scaling

        [MethodImpl(ImplOptions)]
        public static byte ToUInt1(float value)
        {
            return (byte)(value * UInt1Max + 0.5f);
        }

        [MethodImpl(ImplOptions)]
        public static byte ToUInt2(float value)
        {
            return (byte)(value * UInt2Max + 0.5f);
        }

        [MethodImpl(ImplOptions)]
        public static byte ToUInt4(float value)
        {
            return (byte)(value * UInt4Max + 0.5f);
        }

        [MethodImpl(ImplOptions)]
        public static byte ToUInt5(float value)
        {
            return (byte)(value * UInt5Max + 0.5f);
        }

        [MethodImpl(ImplOptions)]
        public static byte ToUInt6(float value)
        {
            return (byte)(value * UInt6Max + 0.5f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static sbyte ToInt8(float value)
        {
            value = Math.Clamp(value, 0, 1);
            value *= byte.MaxValue;
            value += sbyte.MinValue;
            return (sbyte)MathF.Round(value);
        }

        [MethodImpl(ImplOptions)]
        public static byte ToUInt8(float value)
        {
            value *= byte.MaxValue;
            value += 0.5f;
            return MathHelper.ClampTruncate(value, byte.MinValue, byte.MaxValue);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static short ToInt16(float value)
        {
            value = Math.Clamp(value, 0, 1);
            value *= ushort.MaxValue;
            value += short.MinValue;
            return (short)MathF.Round(value);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16(float value)
        {
            value *= ushort.MaxValue;
            value += 0.5f;
            return MathHelper.ClampTruncate(value, ushort.MinValue, ushort.MaxValue);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static uint ToUInt32(double value)
        {
            value *= uint.MaxValue;
            value += 0.5f;
            return MathHelper.ClampTruncate(value, uint.MinValue, uint.MaxValue);
        }

        #endregion

        #region Integer scaling

        #region UInt1

        [MethodImpl(ImplOptions)]
        public static byte ToUInt1(byte value)
        {
            return (byte)(ToFloat32(value) * UInt1Max + 0.5f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt1(ushort value)
        {
            return (byte)(ToFloat32(value) * UInt1Max + 0.5f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt1(uint value)
        {
            return (byte)(ToFloat64(value) * UInt1Max + 0.5f);
        }

        #endregion

        #region UInt2

        [MethodImpl(ImplOptions)]
        public static byte ToUInt2(byte value)
        {
            return (byte)(ToFloat32(value) * UInt2Max + 0.5f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt2(ushort value)
        {
            return (byte)(ToFloat32(value) * UInt2Max + 0.5f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt2(uint value)
        {
            return (byte)(ToFloat64(value) * UInt2Max + 0.5f);
        }

        #endregion

        #region UInt4

        [MethodImpl(ImplOptions)]
        public static byte ToUInt4(byte value)
        {
            return (byte)(ToFloat32(value) * UInt4Max + 0.5f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt4(ushort value)
        {
            return (byte)(ToFloat32(value) * UInt4Max + 0.5f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt4(uint value)
        {
            return (byte)(ToFloat64(value) * UInt4Max + 0.5f);
        }

        #endregion

        #region UInt5

        [MethodImpl(ImplOptions)]
        public static byte ToUInt5(byte value)
        {
            return (byte)(ToFloat32(value) * UInt5Max + 0.5f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt5(ushort value)
        {
            return (byte)(ToFloat32(value) * UInt5Max + 0.5f);
        }

        #endregion

        #region UInt6

        [MethodImpl(ImplOptions)]
        public static byte ToUInt6(byte value)
        {
            return (byte)Math.Round(value / 4f);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt6(ushort value)
        {
            return (byte)Math.Round(value / 1040f);
        }

        #endregion

        #region Int8

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static sbyte ToInt8(byte value)
        {
            return (sbyte)(value + sbyte.MinValue);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static sbyte ToInt8(ushort value)
        {
            return ToInt8(ToUInt8(value));
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static sbyte ToInt8(short value)
        {
            return ToInt8(ToUInt8(value));
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static sbyte ToInt8(uint value)
        {
            return ToInt8(ToUInt8(value));
        }

        #endregion

        #region UInt8

        [MethodImpl(ImplOptions)]
        public static byte ToUInt8From1(byte value)
        {
            return (byte)(value * byte.MaxValue);
        }

        [MethodImpl(ImplOptions)]
        public static byte ToUInt8From2(byte value)
        {
            return (byte)(value * 85);
        }

        [MethodImpl(ImplOptions)]
        public static byte ToUInt8From4(byte value)
        {
            return (byte)(value * 17);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt8(sbyte value)
        {
            return (byte)(value - sbyte.MinValue);
        }

        /// <summary>
        /// Scales a value from a 16-bit <see cref="ushort"/> to it's 8-bit <see cref="byte"/> equivalent.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt8(ushort value)
        {
            return (byte)(((value * 255) + 32895) >> 16);
        }

        [MethodImpl(ImplOptions)]
        public static byte ToUInt8(short value)
        {
            return ToUInt8(ToUInt16(value));
        }

        /// <summary>
        /// Scales a value from an 32-bit <see cref="uint"/> to it's 8-bit <see cref="byte"/> equivalent.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static byte ToUInt8(uint value)
        {
            return (byte)Math.Round(value / 16843009d);
        }

        #endregion

        #region Int16

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static short ToInt16(byte value)
        {
            return ToInt16(ToUInt16(value));
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static short ToInt16(ushort value)
        {
            return (short)(value + short.MinValue);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static short ToInt16(uint value)
        {
            return ToInt16(ToUInt16(value));
        }

        #endregion

        #region UInt16

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16From1(byte value)
        {
            return (ushort)(value * ushort.MaxValue);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16From2(byte value)
        {
            return (ushort)(value * 21845);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16From4(byte value)
        {
            return (ushort)(value * 4369);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16(sbyte value)
        {
            return ToUInt16(ToUInt8(value));
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16(byte value)
        {
            return (ushort)(value * 257);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16(short value)
        {
            return (ushort)(value - short.MinValue);
        }

        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static ushort ToUInt16(uint value)
        {
            return (ushort)Math.Round(value / 65537d);
        }

        #endregion

        #region UInt32

        /// <summary>
        /// Scales a value from an 8-bit <see cref="byte"/> to it's 32-bit <see cref="uint"/> equivalent.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static uint ToUInt32(byte value)
        {
            return value * 16843009u;
        }

        /// <summary>
        /// Scales a value from an 16-bit <see cref="ushort"/> to it's 32-bit <see cref="uint"/> equivalent.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(ImplOptions)]
        public static uint ToUInt32(ushort value)
        {
            return value * 65537u;
        }

        #endregion

        #endregion
    }
}

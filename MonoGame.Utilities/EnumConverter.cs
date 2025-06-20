using System;
using System.Runtime.CompilerServices;

namespace MonoGame.Framework
{
    public static class EnumConverter
    {
        public static long ToInt64<TEnum>(TEnum value)
            where TEnum : unmanaged, Enum
        {
            return Unsafe.SizeOf<TEnum>() switch
            {
                sizeof(sbyte) => Unsafe.BitCast<TEnum, sbyte>(value),
                sizeof(short) => Unsafe.BitCast<TEnum, short>(value),
                sizeof(int) => Unsafe.BitCast<TEnum, int>(value),
                _ => Unsafe.BitCast<TEnum, long>(value)
            };
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64<TEnum>(TEnum value)
            where TEnum : unmanaged, Enum
        {
            return Unsafe.SizeOf<TEnum>() switch
            {
                sizeof(byte) => Unsafe.BitCast<TEnum, byte>(value),
                sizeof(ushort) => Unsafe.BitCast<TEnum, ushort>(value),
                sizeof(uint) => Unsafe.BitCast<TEnum, uint>(value),
                _ => Unsafe.BitCast<TEnum, ulong>(value)
            };
        }
        
        public static TEnum ToEnum<TEnum>(long value)
            where TEnum : unmanaged, Enum
        {
            return Unsafe.SizeOf<TEnum>() switch
            {
                sizeof(sbyte) => Unsafe.BitCast<sbyte, TEnum>((sbyte) value),
                sizeof(short) => Unsafe.BitCast<short, TEnum>((short) value),
                sizeof(int) => Unsafe.BitCast<int, TEnum>((int) value),
                _ => Unsafe.BitCast<long, TEnum>(value)
            };
        }

        [CLSCompliant(false)]
        public static TEnum ToEnum<TEnum>(ulong value)
            where TEnum : unmanaged, Enum
        {
            return Unsafe.SizeOf<TEnum>() switch
            {
                sizeof(byte) => Unsafe.BitCast<byte, TEnum>((byte) value),
                sizeof(ushort) => Unsafe.BitCast<ushort, TEnum>((ushort) value),
                sizeof(uint) => Unsafe.BitCast<uint, TEnum>((uint) value),
                _ => Unsafe.BitCast<ulong, TEnum>(value)
            };
        }
    }
}

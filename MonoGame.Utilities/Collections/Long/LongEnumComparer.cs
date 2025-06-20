using System;
using System.Diagnostics.CodeAnalysis;

namespace MonoGame.Framework.Collections
{
    internal sealed class LongEnumComparer<TEnum> : LongEqualityComparer<TEnum>
        where TEnum : unmanaged, Enum
    {
        public override long GetLongHashCode([DisallowNull] TEnum value)
        {
            return EnumConverter.ToInt64(value);
        }
    }
}
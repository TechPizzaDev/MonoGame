using System;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Collections
{
    internal sealed class LongStringComparer : LongEqualityComparer<string?>
    {
        public override bool IsRandomized => true;

        public override int GetHashCode(string? value) => Hash(value).ToHashCode32();

        public override long GetLongHashCode(string? value) => Hash(value).ToHashCode();

        private static LongHashCode Hash(string? value)
        {
            LongHashCode code = new();
            var bytes = MemoryMarshal.AsBytes(value.AsSpan());
            code.AddBytes(bytes);
            return code;
        }
    }
}
// Copied from .NET Foundation (and Modified)
// See https://github.com/dotnet/runtime/blob/7baa054334c3307652df3ae375d800dc17b1e39f/src/libraries/System.Private.CoreLib/src/System/HashCode.cs
// See https://github.com/dotnet/runtime/blob/7baa054334c3307652df3ae375d800dc17b1e39f/src/libraries/System.IO.Hashing/src/System/IO/Hashing/XxHash64.State.cs

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MonoGame.Framework.Collections;

namespace MonoGame.Framework
{
    [SuppressMessage("Usage", "CA2231:Overload operator equals on overriding value type Equals", Justification = "<Pending>")]
    public partial struct LongHashCode
    {
        private static readonly ulong _seed = (ulong)Random.Shared.NextInt64(long.MinValue, long.MaxValue);

        private const ulong Prime1 = 0x9E3779B185EBCA87UL;
        private const ulong Prime2 = 0xC2B2AE3D27D4EB4FUL;
        private const ulong Prime3 = 0x165667B19E3779F9UL;
        private const ulong Prime4 = 0x85EBCA77C2B2AE63UL;
        private const ulong Prime5 = 0x27D4EB2F165667C5UL;

        private ulong _v1, _v2, _v3, _v4;
        private ulong _queue1, _queue2, _queue3;
        private ulong _length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetLongHashCode<T>(T value)
        {
            return value != null ? LongEqualityComparer<T>.Default.GetLongHashCode(value) : 0;
        }

        public static long Combine<T1>(
            T1 value1)
        {
            // Provide a way of diffusing bits from something with a limited
            // input hash space. For example, many enums only have a few
            // possible hashes, only using the bottom few bits of the code. Some
            // collections are built on the assumption that hashes are spread
            // over a larger space, so diffusing the bits may help the
            // collection work more efficiently.

            ulong hc1 = (ulong)GetLongHashCode(value1);

            ulong hash = MixEmptyState();
            hash += sizeof(ulong);

            hash = QueueRound(hash, hc1);

            hash = MixFinal(hash);
            return (long)hash;
        }

        public static long Combine<T1, T2>(
            T1 value1, T2 value2)
        {
            ulong hc1 = (ulong)GetLongHashCode(value1);
            ulong hc2 = (ulong)GetLongHashCode(value2);

            ulong hash = MixEmptyState();
            hash += sizeof(ulong) * 2;

            hash = QueueRound(hash, hc1);
            hash = QueueRound(hash, hc2);

            hash = MixFinal(hash);
            return (long)hash;
        }

        public static long Combine<T1, T2, T3>(
            T1 value1, T2 value2, T3 value3)
        {
            ulong hc1 = (ulong)GetLongHashCode(value1);
            ulong hc2 = (ulong)GetLongHashCode(value2);
            ulong hc3 = (ulong)GetLongHashCode(value3);

            ulong hash = MixEmptyState();
            hash += sizeof(ulong) * 3;

            hash = QueueRound(hash, hc1);
            hash = QueueRound(hash, hc2);
            hash = QueueRound(hash, hc3);

            hash = MixFinal(hash);
            return (long)hash;
        }

        public static long Combine<T1, T2, T3, T4>(
            T1 value1, T2 value2, T3 value3, T4 value4)
        {
            ulong hc1 = (ulong)GetLongHashCode(value1);
            ulong hc2 = (ulong)GetLongHashCode(value2);
            ulong hc3 = (ulong)GetLongHashCode(value3);
            ulong hc4 = (ulong)GetLongHashCode(value4);

            Initialize(out ulong v1, out ulong v2, out ulong v3, out ulong v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            ulong hash = MixState(v1, v2, v3, v4);
            hash += sizeof(ulong) * 4;

            hash = MixFinal(hash);
            return (long)hash;
        }

        public static long Combine<T1, T2, T3, T4, T5>(
            T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            ulong hc1 = (ulong)GetLongHashCode(value1);
            ulong hc2 = (ulong)GetLongHashCode(value2);
            ulong hc3 = (ulong)GetLongHashCode(value3);
            ulong hc4 = (ulong)GetLongHashCode(value4);
            ulong hc5 = (ulong)GetLongHashCode(value5);

            Initialize(out ulong v1, out ulong v2, out ulong v3, out ulong v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            ulong hash = MixState(v1, v2, v3, v4);
            hash += sizeof(ulong) * 5;

            hash = QueueRound(hash, hc5);

            hash = MixFinal(hash);
            return (long)hash;
        }

        public static long Combine<T1, T2, T3, T4, T5, T6>(
            T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            ulong hc1 = (ulong)GetLongHashCode(value1);
            ulong hc2 = (ulong)GetLongHashCode(value2);
            ulong hc3 = (ulong)GetLongHashCode(value3);
            ulong hc4 = (ulong)GetLongHashCode(value4);
            ulong hc5 = (ulong)GetLongHashCode(value5);
            ulong hc6 = (ulong)GetLongHashCode(value6);

            Initialize(out ulong v1, out ulong v2, out ulong v3, out ulong v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            ulong hash = MixState(v1, v2, v3, v4);
            hash += sizeof(ulong) * 6;

            hash = QueueRound(hash, hc5);
            hash = QueueRound(hash, hc6);

            hash = MixFinal(hash);
            return (long)hash;
        }

        public static long Combine<T1, T2, T3, T4, T5, T6, T7>(
            T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
        {
            ulong hc1 = (ulong)GetLongHashCode(value1);
            ulong hc2 = (ulong)GetLongHashCode(value2);
            ulong hc3 = (ulong)GetLongHashCode(value3);
            ulong hc4 = (ulong)GetLongHashCode(value4);
            ulong hc5 = (ulong)GetLongHashCode(value5);
            ulong hc6 = (ulong)GetLongHashCode(value6);
            ulong hc7 = (ulong)GetLongHashCode(value7);

            Initialize(out ulong v1, out ulong v2, out ulong v3, out ulong v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            ulong hash = MixState(v1, v2, v3, v4);
            hash += sizeof(ulong) * 7;

            hash = QueueRound(hash, hc5);
            hash = QueueRound(hash, hc6);
            hash = QueueRound(hash, hc7);

            hash = MixFinal(hash);
            return (long)hash;
        }

        public static long Combine<T1, T2, T3, T4, T5, T6, T7, T8>(
            T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
        {
            ulong hc1 = (ulong)GetLongHashCode(value1);
            ulong hc2 = (ulong)GetLongHashCode(value2);
            ulong hc3 = (ulong)GetLongHashCode(value3);
            ulong hc4 = (ulong)GetLongHashCode(value4);
            ulong hc5 = (ulong)GetLongHashCode(value5);
            ulong hc6 = (ulong)GetLongHashCode(value6);
            ulong hc7 = (ulong)GetLongHashCode(value7);
            ulong hc8 = (ulong)GetLongHashCode(value8);

            Initialize(out ulong v1, out ulong v2, out ulong v3, out ulong v4);

            v1 = Round(v1, hc1);
            v2 = Round(v2, hc2);
            v3 = Round(v3, hc3);
            v4 = Round(v4, hc4);

            v1 = Round(v1, hc5);
            v2 = Round(v2, hc6);
            v3 = Round(v3, hc7);
            v4 = Round(v4, hc8);

            ulong hash = MixState(v1, v2, v3, v4);
            hash += sizeof(ulong) * 8;

            hash = MixFinal(hash);
            return (long)hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(out ulong v1, out ulong v2, out ulong v3, out ulong v4)
        {
            v1 = _seed + unchecked(Prime1 + Prime2);
            v2 = _seed + Prime2;
            v3 = _seed;
            v4 = _seed - Prime1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Round(ulong hash, ulong input)
        {
            return BitOperations.RotateLeft(hash + input * Prime2, 31) * Prime1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong QueueRound(ulong hash, ulong queuedValue)
        {
            return (hash ^ Round(0, queuedValue)) * Prime1 + Prime4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MixState(ulong v1, ulong v2, ulong v3, ulong v4)
        {
            return
                BitOperations.RotateLeft(v1, 1) +
                BitOperations.RotateLeft(v2, 7) +
                BitOperations.RotateLeft(v3, 12) +
                BitOperations.RotateLeft(v4, 18);
        }

        private static ulong MixEmptyState()
        {
            return _seed + Prime5;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MixFinal(ulong hash)
        {
            hash ^= hash >> 33;
            hash *= Prime2;
            hash ^= hash >> 29;
            hash *= Prime3;
            hash ^= hash >> 32;
            return hash;
        }

        public void Add<T>(T value)
        {
            Add(GetLongHashCode(value));
        }

        public void Add<T>(T value, ILongEqualityComparer<T>? comparer)
        {
            comparer ??= LongEqualityComparer<T>.Default;
            Add(value == null ? 0 : comparer.GetLongHashCode(value));
        }

        /// <summary>Adds a span of bytes to the hash code.</summary>
        /// <param name="value">The span.</param>
        /// <remarks>
        /// This method does not guarantee that the result of adding a span of bytes will match
        /// the result of adding the same bytes individually.
        /// </remarks>
        public void AddBytes(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(long) * 4)
            {
                goto Small;
            }

            // Usually Add calls Initialize but if we haven't used HashCode before it won't have been called.
            if (_length == 0)
            {
                Initialize(out _v1, out _v2, out _v3, out _v4);
            }
            else
            {
                // If we have at least 32 bytes to hash, we can add them in 32-byte batches,
                // but we first have to add enough data to flush any queued values.
                switch (_length % 4)
                {
                    case 1:
                        Add(MemoryMarshal.Read<long>(value));
                        value = value.Slice(sizeof(long));
                        goto case 2;
                    case 2:
                        Add(MemoryMarshal.Read<long>(value));
                        value = value.Slice(sizeof(long));
                        goto case 3;
                    case 3:
                        Add(MemoryMarshal.Read<long>(value));
                        value = value.Slice(sizeof(long));
                        break;
                }
            }

            // With the queue clear, we add 32 bytes at a time until the input has fewer than 32 bytes remaining.
            while (value.Length >= sizeof(long) * 4)
            {
                _v1 = Round(_v1, MemoryMarshal.Read<ulong>(value));
                _v2 = Round(_v2, MemoryMarshal.Read<ulong>(value.Slice(sizeof(long) * 1)));
                _v3 = Round(_v3, MemoryMarshal.Read<ulong>(value.Slice(sizeof(long) * 2)));
                _v4 = Round(_v4, MemoryMarshal.Read<ulong>(value.Slice(sizeof(long) * 3)));

                _length += 4;
                value = value.Slice(sizeof(long) * 4);
            }

        Small:
            // Add 8 bytes at a time until the input has fewer than 8 bytes remaining.
            while (value.Length >= sizeof(long))
            {
                Add(MemoryMarshal.Read<long>(value));
                value = value.Slice(sizeof(long));
            }
        
            // Add the remaining bytes in bulk.
            Span<byte> remainder = stackalloc byte[sizeof(long)];
            value.CopyTo(remainder);
            Add(MemoryMarshal.Read<long>(remainder));
        }

        private void Add(long value)
        {
            // The original xxHash works as follows:
            // 0. Initialize immediately. We can't do this in a struct (no
            //    default ctor).
            // 1. Accumulate blocks of length 32 (4 ulongs) into 4 accumulators.
            // 2. Accumulate remaining blocks of length 8 (1 ulong) into the
            //    hash.
            // 3. Accumulate remaining blocks of length 1 into the hash.

            // There is no need for #3 as this type only accepts longs. _queue1,
            // _queue2 and _queue3 are basically a buffer so that when
            // ToHashCode is called we can execute #2 correctly.

            // We need to initialize the xxHash64 state (_v1 to _v4) lazily (see
            // #0) nd the last place that can be done if you look at the
            // original code is just before the first block of 16 bytes is mixed
            // in. The xxHash64 state is never used for streams containing fewer
            // than 32 bytes.

            // To see what's really going on here, have a look at the Combine
            // methods.

            ulong val = (ulong)value;

            // Storing the value of _length locally shaves of quite a few bytes
            // in the resulting machine code.
            ulong previousLength = _length++;
            ulong position = previousLength % 4;

            // Switch can't be inlined.

            if (position == 0)
                _queue1 = val;
            else if (position == 1)
                _queue2 = val;
            else if (position == 2)
                _queue3 = val;
            else // position == 3
            {
                if (previousLength == 3)
                    Initialize(out _v1, out _v2, out _v3, out _v4);

                _v1 = Round(_v1, _queue1);
                _v2 = Round(_v2, _queue2);
                _v3 = Round(_v3, _queue3);
                _v4 = Round(_v4, val);
            }
        }

        public readonly long ToHashCode()
        {
            // Storing the value of _length locally shaves of quite a few bytes
            // in the resulting machine code.
            ulong length = _length;

            // position refers to the *next* queue position in this method, so
            // position == 1 means that _queue1 is populated; _queue2 would have
            // been populated on the next call to Add.
            ulong position = length % 4;

            // If the length is less than 8, _v1 to _v4 don't contain anything
            // yet. xxHash64 treats this differently.

            ulong hash = length < 4 ? MixEmptyState() : MixState(_v1, _v2, _v3, _v4);

            // _length is incremented once per Add(Int64) and is therefore 8
            // times too small (xxHash length is in bytes, not ints).

            hash += sizeof(ulong) * length;

            // Mix what remains in the queue

            // Switch can't be inlined right now, so use as few branches as
            // possible by manually excluding impossible scenarios (position > 1
            // is always false if position is not > 0).
            if (position > 0)
            {
                hash = QueueRound(hash, _queue1);
                if (position > 1)
                {
                    hash = QueueRound(hash, _queue2);
                    if (position > 2)
                        hash = QueueRound(hash, _queue3);
                }
            }

            hash = MixFinal(hash);
            return (long)hash;
        }

        public readonly int ToHashCode32()
        {
            long hash = ToHashCode();
            return (int)hash ^ (int)(hash >>> 32);
        }

#pragma warning disable 0809
        // Obsolete member 'memberA' overrides non-obsolete member 'memberB'.
        // Disallowing GetHashCode and Equals is by design

        // * We decided to not override GetHashCode() to produce the hash code
        //   as this would be weird, both naming-wise as well as from a
        //   behavioral standpoint (GetHashCode() should return the object's
        //   hash code, not the one being computed).

        // * Even though ToHashCode() can be called safely multiple times on
        //   this implementation, it is not part of the contract. If the
        //   implementation has to change in the future we don't want to worry
        //   about people who might have incorrectly used this type.

        [Obsolete(
            "LongHashCode is a mutable struct and should not be compared with other LongHashCodes. " +
            "Use ToHashCode to retrieve the computed hash code.", error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            throw new NotSupportedException();
        }

        [Obsolete("LongHashCode is a mutable struct and should not be compared with other LongHashCodes.", error: true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
        {
            throw new NotSupportedException();
        }
#pragma warning restore 0809
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MonoGame.Framework
{
    public ref struct RuneEnumerator
    {
        private SpanRuneEnumerator _spanEnumerator;
        private StringBuilder.ChunkEnumerator _builderEnumerator;
        private readonly IEnumerator<Rune>? _interfaceEnumerator;

        public Rune Current { get; private set; }

        public RuneEnumerator(SpanRuneEnumerator spanEnumerator)
        {
            _spanEnumerator = spanEnumerator;
        }

        public RuneEnumerator(StringBuilder.ChunkEnumerator builderEnumerator)
        {
            _builderEnumerator = builderEnumerator;
        }

        public RuneEnumerator(IEnumerator<Rune>? interfaceEnumerator)
        {
            _interfaceEnumerator = interfaceEnumerator;
        }

        public bool MoveNext()
        {
            if (_spanEnumerator.MoveNext())
            {
                Current = _spanEnumerator.Current;
                return true;
            }
            return MoveNextSlow();
        }

        private bool MoveNextSlow()
        {
            if (_builderEnumerator.MoveNext())
            {
                return MoveNextRefill();
            }

            if (_interfaceEnumerator == null)
            {
                return false;
            }
            return MoveNextInterface();
        }

        private bool MoveNextInterface()
        {
            Debug.Assert(_interfaceEnumerator != null);
            if (_interfaceEnumerator.MoveNext())
            {
                Current = _interfaceEnumerator.Current;
                return true;
            }
            return false;
        }

        private bool MoveNextRefill()
        {
            var chunkSpan = _builderEnumerator.Current.Span;
            _spanEnumerator = chunkSpan.EnumerateRunes();

            if (_spanEnumerator.MoveNext())
            {
                Current = _spanEnumerator.Current;
                return true;
            }
            return false;
        }

        public readonly RuneEnumerator GetEnumerator() => this;

        public static implicit operator RuneEnumerator(ReadOnlySpan<char> text) => new(text.EnumerateRunes());

        public static implicit operator RuneEnumerator(ReadOnlyMemory<char> text) => text.Span;

        public static implicit operator RuneEnumerator(string? text) => text.AsSpan();

        public static implicit operator RuneEnumerator(StringBuilder? text)
        {
            if (text == null)
                return default;
            return new RuneEnumerator(text.GetChunks());
        }
    }
}

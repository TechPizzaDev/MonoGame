// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace MonoGame.Framework.Input
{
    /// <summary>
    /// Holds the state of keystrokes by a keyboard.
    /// </summary>
    public struct KeyboardState : IReadOnlyCollection<Keys>, IEquatable<KeyboardState>
    {
        /// <summary>
        /// Gets the max amount of keystrokes that can
        /// be tracked by a <see cref="KeyboardState"/>. 
        /// </summary>
        public const int MaxKeysPerState = 256;

        // Array of 256 bits:
        private uint _key0, _key1, _key2, _key3, _key4, _key5, _key6, _key7;

        #region Properties

        /// <summary>
        /// Returns the state of a specified key.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>The state of the key.</returns>
        public readonly KeyState this[Keys key] => GetKey(key) ? KeyState.Down : KeyState.Up;

        /// <summary>
        /// Gets the active key modifiers.
        /// </summary>
        public KeyModifiers Modifiers { get; }

        /// <summary>
        /// Gets the state of the Caps Lock key.
        /// </summary>
        public readonly bool CapsLock => (Modifiers & KeyModifiers.CapsLock) != 0;

        /// <summary>
        /// Gets the state of the Num Lock key.
        /// </summary>
        public readonly bool NumLock => (Modifiers & KeyModifiers.NumLock) != 0;

        /// <summary>
        /// Gets the combined state of left and right Control key.
        /// </summary>
        public readonly bool Control => (Modifiers & KeyModifiers.Control) != 0;

        /// <summary>
        /// Gets the combined state of left and right Alt key.
        /// </summary>
        public readonly bool Alt => (Modifiers & KeyModifiers.Alt) != 0;

        /// <summary>
        /// Gets the amount of pressed keys.
        /// </summary>
        public readonly int Count
        {
            get
            {
                return
                    BitOperations.PopCount(_key0) +
                    BitOperations.PopCount(_key1) +
                    BitOperations.PopCount(_key2) +
                    BitOperations.PopCount(_key3) +
                    BitOperations.PopCount(_key4) +
                    BitOperations.PopCount(_key5) +
                    BitOperations.PopCount(_key6) +
                    BitOperations.PopCount(_key7);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        /// <param name="keys">List of keys to be flagged as pressed.</param>
        /// <param name="modifiers">Active key modifiers.</param>
        public KeyboardState(IEnumerable<Keys> keys, KeyModifiers modifiers) : this()
        {
            Modifiers = modifiers;

            if (keys != null)
            {
                if (keys is IList<Keys> list) // prevents alloc of enumerable
                {
                    for (int i = 0; i < list.Count; i++)
                        SetKey(list[i]);
                }
                else
                {
                    foreach (var key in keys)
                        SetKey(key);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        /// <param name="keys">List of keys to be flagged as pressed.</param>
        /// <param name="modifiers">Active key modifiers.</param>
        public KeyboardState(ReadOnlySpan<Keys> keys, KeyModifiers modifiers) : this()
        {
            Modifiers = modifiers;

            for (int i = 0; i < keys.Length; i++)
                SetKey(keys[i]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardState"/> class.
        /// </summary>
        /// <param name="keys">List of keys to be flagged as pressed.</param>
        /// <param name="modifiers">Active key modifiers.</param>
        public KeyboardState(KeyModifiers modifiers, params Keys[] keys) : this(keys.AsSpan(), modifiers)
        {
        }

        #endregion

        #region Key Data

        private readonly uint GetKeyValue(int index)
        {
            return index switch
            {
                0 => _key0,
                1 => _key1,
                2 => _key2,
                3 => _key3,
                4 => _key4,
                5 => _key5,
                6 => _key6,
                7 => _key7,
                _ => 0
            };
        }

        private readonly bool GetKey(Keys key)
        {
            int index = ((int)key) >> 5;
            uint field = GetKeyValue(index);
            uint mask = (uint)1 << (((int)key) & 0x1f);
            return (field & mask) != 0;
        }

        internal void SetKey(Keys key)
        {
            uint mask = (uint)1 << (((int)key) & 0x1f);
            switch (((int)key) >> 5)
            {
                case 0: _key0 |= mask; break;
                case 1: _key1 |= mask; break;
                case 2: _key2 |= mask; break;
                case 3: _key3 |= mask; break;
                case 4: _key4 |= mask; break;
                case 5: _key5 |= mask; break;
                case 6: _key6 |= mask; break;
                case 7: _key7 |= mask; break;
            }
        }

        internal void ClearKey(Keys key)
        {
            uint mask = (uint)1 << (((int)key) & 0x1f);
            switch (((int)key) >> 5)
            {
                case 0: _key0 &= ~mask; break;
                case 1: _key1 &= ~mask; break;
                case 2: _key2 &= ~mask; break;
                case 3: _key3 &= ~mask; break;
                case 4: _key4 &= ~mask; break;
                case 5: _key5 &= ~mask; break;
                case 6: _key6 &= ~mask; break;
                case 7: _key7 &= ~mask; break;
            }
        }

        internal void ClearAllKeys()
        {
            _key0 = 0;
            _key1 = 0;
            _key2 = 0;
            _key3 = 0;
            _key4 = 0;
            _key5 = 0;
            _key6 = 0;
            _key7 = 0;
        }

        #endregion

        /// <summary>
        /// Gets whether given key is currently being pressed.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>true if the key is pressed; false otherwise.</returns>
        public readonly bool IsKeyDown(Keys key)
        {
            return GetKey(key);
        }

        /// <summary>
        /// Gets whether given key is currently being not pressed.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>true if the key is not pressed; false otherwise.</returns>
        public readonly bool IsKeyUp(Keys key)
        {
            return !GetKey(key);
        }

        /// <summary>
        /// Returns an array with keys that are currently being pressed.
        /// </summary>
        /// <returns>The keys that are currently being pressed.</returns>
        [Obsolete("This method allocates a new array for every call.")]
        public readonly Keys[] GetPressedKeys()
        {
            var keys = new Keys[Count];
            GetPressedKeys(keys);
            return keys;
        }

        /// <summary>
        /// Fills a span with keys that are currently being pressed.
        /// </summary>
        /// <param name="keys">The destination span for the keys.</param>
        /// <returns>The amount of keys that were added to the span.</returns>
        public readonly int GetPressedKeys(Span<Keys> keys)
        {
            int index = 0;
            var enumerator = GetEnumerator();
            while (index < keys.Length && enumerator.MoveNext())
            {
                keys[index] = enumerator.Current;
                index++;
            }
            return index;
        }

        #region Equals

        public readonly bool Equals(KeyboardState other)
        {
            return this == other;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is KeyboardState other && Equals(other);
        }

        public static bool operator ==(in KeyboardState a, in KeyboardState b)
        {
            return a.Modifiers == b.Modifiers
                && a._key0 == b._key0
                && a._key1 == b._key1
                && a._key2 == b._key2
                && a._key3 == b._key3
                && a._key4 == b._key4
                && a._key5 == b._key5
                && a._key6 == b._key6
                && a._key7 == b._key7;
        }

        public static bool operator !=(in KeyboardState a, in KeyboardState b)
        {
            return !(a == b);
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Gets a hash code of this <see cref="KeyboardState"/>.
        /// </summary>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(
                Modifiers,
                HashCode.Combine(_key0, _key1, _key2, _key3, _key4, _key5, _key6, _key7));
        }

        #endregion

        #region IEnumerable, Enumerator

        /// <summary>
        /// Returns an <see cref="Enumerator"/> that enumerates the currently pressed keys.
        /// </summary>
        public readonly Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        readonly IEnumerator<Keys> IEnumerable<Keys>.GetEnumerator() => GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Enumerates the pressed keys of a <see cref="KeyboardState"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<Keys>
        {
            private KeyboardState _state;
            private int _keyIndex;
            private int _iterIndex;

            /// <summary>
            /// Gets the pressed key at the current position of the enumerator.
            /// </summary>
            public Keys Current { get; private set; }

            object IEnumerator.Current => Current;

            /// <summary>
            /// Constructs the <see cref="Enumerator"/>.
            /// </summary>
            /// <param name="state">The <see cref="KeyboardState"/> to enumerate.</param>
            public Enumerator(in KeyboardState state)
            {
                _state = state;
                _keyIndex = 0;
                _iterIndex = 0;
                Current = default;
            }

            /// <summary>
            /// Advances the <see cref="Enumerator"/> to the next pressed key.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the enumerator was successfully advanced to the next pressed key;
            /// <see langword="false"/> if the enumerator has enumerated all the pressed keys.
            /// </returns>
            public bool MoveNext()
            {
                while (_keyIndex < 8)
                {
                    uint keyField = _state.GetKeyValue(_keyIndex);
                    if (keyField != 0)
                    {
                        for (; _iterIndex < 32; _iterIndex++)
                        {
                            if ((keyField & (1 << _iterIndex)) != 0)
                            {
                                int offset = _keyIndex * 32;
                                Current = (Keys)(offset + _iterIndex);

                                // the return exits the loop so increment here too
                                _iterIndex++;
                                return true;
                            }
                        }
                    }
                    _iterIndex = 0;
                    _keyIndex++;
                }
                return false;
            }

            /// <summary>
            /// Resets the <see cref="Enumerator"/>.
            /// </summary>
            public void Reset()
            {
                _keyIndex = 0;
                _iterIndex = 0;
                Current = default;
            }

            void IDisposable.Dispose()
            {
            }
        }

        #endregion
    }
}

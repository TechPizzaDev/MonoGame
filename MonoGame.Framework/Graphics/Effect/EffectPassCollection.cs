using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoGame.Framework.Graphics
{
    public class EffectPassCollection : IEnumerable<EffectPass>
    {
        private readonly EffectPass[] _passes;

        public int Count => _passes.Length;

        public EffectPass this[int index] => _passes[index];

        public EffectPass? this[string name]
        {
            get
            {
                // TODO: Add a name to pass lookup table.
                foreach (var pass in _passes)
                {
                    if (pass.Name == name)
                        return pass;
                }
                return null;
            }
        }

        internal EffectPassCollection(EffectPass[] passes)
        {
            _passes = passes;
        }

        internal EffectPassCollection Clone(Effect effect)
        {
            var passes = new EffectPass[_passes.Length];

            for (var i = 0; i < _passes.Length; i++)
                passes[i] = new EffectPass(effect, _passes[i]);

            return new EffectPassCollection(passes);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_passes);
        }

        IEnumerator<EffectPass> IEnumerable<EffectPass>.GetEnumerator()
        {
            return ((IEnumerable<EffectPass>)_passes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _passes.GetEnumerator();
        }

        public struct Enumerator : IEnumerator<EffectPass>
        {
            private readonly EffectPass[] _array;
            private int _index;

            public EffectPass Current { get; private set; }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == _array.Length + 1)
                        throw new InvalidOperationException();
                    return Current;
                }
            }

            internal Enumerator(EffectPass[] array)
            {
                _array = array;
                _index = 0;
                Current = null!;
            }

            public bool MoveNext()
            {
                int index = _index;
                var array = _array;
                if ((uint) index < (uint) array.Length)
                {
                    Current = array[index];
                    _index = index + 1;
                    return true;
                }

                _index = array.Length + 1;
                Current = null!;
                return false;
            }

            public void Reset()
            {
                _index = 0;
                Current = null!;
            }

            public void Dispose()
            {
            }
        }
    }
}

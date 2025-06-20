using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoGame.Framework.Graphics
{
    public class EffectParameterCollection : IReadOnlyList<EffectParameter>
    {
        internal static readonly EffectParameterCollection Empty = new(Array.Empty<EffectParameter>());

        private readonly EffectParameter[] _parameters;
        private readonly Dictionary<string, int> _indexLookup;

        public int Count => _parameters.Length;

        public EffectParameter this[int index] => _parameters[index];

        public EffectParameter? this[string name]
        {
            get
            {
                if (_indexLookup.TryGetValue(name, out int index))
                    return _parameters[index];
                return null;
            }
        }

        internal EffectParameterCollection(EffectParameter[] parameters)
        {
            _parameters = parameters;
            _indexLookup = new Dictionary<string, int>(_parameters.Length);
            for (int i = 0; i < _parameters.Length; i++)
            {
                string name = _parameters[i].Name;
                if(!string.IsNullOrWhiteSpace(name))
                    _indexLookup.Add(name, i);
            }
        }

        private EffectParameterCollection(EffectParameter[] parameters, Dictionary<string, int> indexLookup)
        {
            _parameters = parameters;
            _indexLookup = indexLookup;
        }

        internal EffectParameterCollection Clone()
        {
            if (_parameters.Length == 0)
                return Empty;

            var parameters = new EffectParameter[_parameters.Length];
            for (var i = 0; i < _parameters.Length; i++)
                parameters[i] = new EffectParameter(_parameters[i]);

            return new EffectParameterCollection(parameters, _indexLookup);
        }

        public IEnumerator<EffectParameter> GetEnumerator() => ((IEnumerable<EffectParameter>)_parameters).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _parameters.GetEnumerator();
    }
}

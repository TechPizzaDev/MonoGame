using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MonoGame.Imaging.Config.Providers
{
    public class ImagingInstanceProvider<TInstance> : IReadOnlyDictionary<ImageFormat, TInstance>
    {
        private readonly ConcurrentDictionary<ImageFormat, TInstance> _instances = new();

        public int Count => _instances.Count;

        public IEnumerable<ImageFormat> Keys => _instances.Keys;
        public IEnumerable<TInstance> Values => _instances.Values;

        public TInstance this[ImageFormat key] => _instances[key];

        public bool TryAdd(ImageFormat key, TInstance instance)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (instance == null)
                throw new ArgumentNullException(nameof(key));
            return _instances.TryAdd(key, instance);
        }

        public bool ContainsKey(ImageFormat key)
        {
            return _instances.ContainsKey(key);
        }

        public bool TryGetValue(ImageFormat key, [MaybeNullWhen(false)] out TInstance value)
        {
            return _instances.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<ImageFormat, TInstance>> GetEnumerator()
        {
            return _instances.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace MonoGame.Framework.Content
{
    internal class DictionaryReader<TKey, TValue> : ContentTypeReader<Dictionary<TKey, TValue>>
        where TKey : notnull
    {
        ContentTypeReader? keyReader;
        ContentTypeReader? valueReader;
        
        public DictionaryReader()
        {
        }

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            keyReader = manager.GetTypeReader(typeof(TKey));
            valueReader = manager.GetTypeReader(typeof(TValue));
        }

        public override bool CanDeserializeIntoExistingObject => true;

        protected internal override Dictionary<TKey, TValue> Read(ContentReader input, Dictionary<TKey, TValue> existingInstance)
        {
            int count = input.ReadInt32();

            Dictionary<TKey, TValue> dictionary = existingInstance;
            if (dictionary == null)
                dictionary = new Dictionary<TKey, TValue>(count);
            else
                dictionary.Clear();

            for (int i = 0; i < count; i++)
            {
                TKey? key;
                TValue? value;

                if (typeof(TKey).IsValueType)
                {
                    key = input.ReadObject<TKey>(keyReader);
                }
                else
                {
                    var readerType = input.Read7BitEncodedInt();
                    key = readerType > 0 ? input.ReadObject<TKey>(input.TypeReaders[readerType - 1]) : default;
                }

                if (typeof(TValue).IsValueType)
                {
                    value = input.ReadObject<TValue>(valueReader);
                }
                else
                {
                    var readerType = input.Read7BitEncodedInt();
                    value = readerType > 0 ? input.ReadObject<TValue>(input.TypeReaders[readerType - 1]) : default;
                }

                dictionary.Add(key, value);
            }
            return dictionary;
        }
    }
}


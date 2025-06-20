// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;

namespace MonoGame.Framework.Content
{
    internal class ArrayReader<T> : ContentTypeReader<T?[]>
    {
        private ContentTypeReader? _elementReader;

        public ArrayReader()
        {
        }

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            _elementReader = manager.GetTypeReader<T>();
        }

        protected internal override T?[] Read(ContentReader input, T?[] existingInstance)
        {
            uint count = input.ReadUInt32();

            ContentTypeReader[]? readers = input.TypeReaders;
            Debug.Assert(readers != null);

            T?[] array = existingInstance ?? new T[count];

            if (typeof(T).IsValueType)
            {
                for (uint i = 0; i < count; i++)
                    array[i] = input.ReadObject<T>(_elementReader);
            }
            else
            {
                for (uint i = 0; i < count; i++)
                {
                    int readerType = input.Read7BitEncodedInt();
                    array[i] = readerType > 0 ? input.ReadObject<T>(readers[readerType - 1]) : default;
                }
            }
            return array;
        }
    }
}

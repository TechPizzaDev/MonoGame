// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using MonoGame.Framework.Utilities;

namespace MonoGame.Framework.Content
{
    public sealed class ContentReader : BinaryReader
    {
        private Action<IDisposable>? _recordDisposableObject;
        private ContentTypeReaderManager? _typeReaderManager;
        private List<KeyValuePair<int, Action<object>>> _sharedResourceFixups = new();

        internal int Version { get; private set; }
        internal int SharedResourceCount { get; private set; }

        internal ContentTypeReader[]? TypeReaders { get; private set; }

        public ContentManager ContentManager { get; }
        public string AssetName { get; }

        internal ContentReader(
            ContentManager manager, Stream stream,
            string assetName, int version, Action<IDisposable>? recordDisposableObject)
            : base(stream)
        {
            ContentManager = manager;
            AssetName = assetName;
            Version = version;
            _recordDisposableObject = recordDisposableObject;
        }

        internal T? ReadAsset<T>()
        {
            InitializeTypeReaders();

            // Read primary object
            T? result = ReadObject<T>();

            // Read shared resources
            ReadSharedResources();

            return result;
        }

        internal T? ReadAsset<T>(T? existingInstance)
        {
            InitializeTypeReaders();

            // Read primary object
            T? result = ReadObject(existingInstance);

            // Read shared resources
            ReadSharedResources();

            return result;
        }

        internal void InitializeTypeReaders()
        {
            _typeReaderManager = new ContentTypeReaderManager();
            TypeReaders = _typeReaderManager.LoadAssetReaders(this);
            SharedResourceCount = Read7BitEncodedInt();
        }

        internal void ReadSharedResources()
        {
            if (SharedResourceCount <= 0)
                return;

            var sharedResources = new object?[SharedResourceCount];
            for (var i = 0; i < SharedResourceCount; ++i)
                sharedResources[i] = ReadObject(default(object));

            // Fixup shared resources by calling each registered action
            foreach (var fixup in _sharedResourceFixups)
            {
                fixup.Value.Invoke(sharedResources[fixup.Key]);
            }
        }

        public T? ReadExternalReference<T>()
        {
            string externalReference = ReadString();

            if (!string.IsNullOrEmpty(externalReference))
                return ContentManager.Load<T>(
                    FileHelpers.ResolveRelativePath(AssetName, externalReference));

            return default;
        }

        private void RecordDisposable<T>(T result)
        {
            if (result is not IDisposable disposable)
                return;

            if (_recordDisposableObject != null)
                _recordDisposableObject.Invoke(disposable);
            else
                ContentManager.RecordDisposable(disposable);
        }

        public T? ReadObject<T>()
        {
            return ReadObject(default(T));
        }

        public T? ReadObject<T>(T? existingInstance)
        {
            int typeReaderIndex = Read7BitEncodedInt();
            if (typeReaderIndex == 0)
                return existingInstance;

            Debug.Assert(TypeReaders != null);
            if (typeReaderIndex > TypeReaders.Length)
                throw new ContentLoadException("Incorrect type reader index found.");

            var typeReader = TypeReaders[typeReaderIndex - 1];
            var result = (T?) typeReader.Read(this, existingInstance);

            RecordDisposable(result);
            return result;
        }

        public T? ReadObject<T>(ContentTypeReader typeReader)
        {
            var result = (T?) typeReader.Read(this, default(T));
            RecordDisposable(result);
            return result;
        }

        public T? ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
            if (!typeReader.TargetType.IsValueType)
                return ReadObject(existingInstance);

            T? result = (T?) typeReader.Read(this, existingInstance);

            RecordDisposable(result);
            return result;
        }

        public T? ReadRawObject<T>()
        {
            return ReadRawObject(default(T));
        }

        public T? ReadRawObject<T>(ContentTypeReader typeReader)
        {
            return ReadRawObject<T>(typeReader, default);
        }

        public T? ReadRawObject<T>(T? existingInstance)
        {
            Debug.Assert(TypeReaders != null);
            foreach (var typeReader in TypeReaders)
            {
                if (typeReader.TargetType == typeof(T))
                    return ReadRawObject(typeReader, existingInstance);
            }
            throw new NotSupportedException();
        }

        public T? ReadRawObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
            return (T?) typeReader.Read(this, existingInstance);
        }

        public void ReadSharedResource<T>(Action<T> fixup)
        {
            int index = Read7BitEncodedInt();
            if (index <= 0)
                return;

            _sharedResourceFixups.Add(
                new KeyValuePair<int, Action<object>>(index - 1, delegate (object v)
            {
                if (v is not T)
                {
                    throw new ContentLoadException(
                        $"Error loading shared resource. Expected type {typeof(T).Name}, received type {v.GetType().Name}.");
                }
                fixup.Invoke((T) v);
            }));
        }

        #region Read helpers

        public Matrix4x4 ReadMatrix4x4()
        {
            return new Matrix4x4(
                m11: ReadSingle(),
                m12: ReadSingle(),
                m13: ReadSingle(),
                m14: ReadSingle(),
                m21: ReadSingle(),
                m22: ReadSingle(),
                m23: ReadSingle(),
                m24: ReadSingle(),
                m31: ReadSingle(),
                m32: ReadSingle(),
                m33: ReadSingle(),
                m34: ReadSingle(),
                m41: ReadSingle(),
                m42: ReadSingle(),
                m43: ReadSingle(),
                m44: ReadSingle());
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(
                x: ReadSingle(),
                y: ReadSingle(),
                z: ReadSingle(),
                w: ReadSingle());
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(
                x: ReadSingle(),
                y: ReadSingle());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(
                x: ReadSingle(),
                y: ReadSingle(),
                z: ReadSingle());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(
                x: ReadSingle(),
                y: ReadSingle(),
                z: ReadSingle(),
                w: ReadSingle());
        }

        public Color ReadColor()
        {
            return new Color(
                r: ReadByte(),
                g: ReadByte(),
                b: ReadByte(),
                a: ReadByte());
        }

        public BoundingSphere ReadBoundingSphere()
        {
            var position = ReadVector3();
            var radius = ReadSingle();
            return new BoundingSphere(position, radius);
        }

        #endregion
    }
}

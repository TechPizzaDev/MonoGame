// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MonoGame.Framework.Utilities;

namespace MonoGame.Framework.Content
{
    public class ReflectiveReader<T> : ContentTypeReader
    {
        delegate void ReadElement(ContentReader input, object parent);

        private List<ReadElement>? _readers;
        private ConstructorInfo? _constructor;
        private ContentTypeReader? _baseTypeReader;


        public ReflectiveReader()  : base(typeof(T))
        {
        }

        public override bool CanDeserializeIntoExistingObject => TargetType.IsClass;

        protected internal override void Initialize(ContentTypeReaderManager manager)
        {
            base.Initialize(manager);

            var baseType = TargetType.BaseType;
            if (baseType != null && baseType != typeof(object))
                _baseTypeReader = manager.GetTypeReader(baseType);

            _constructor = TargetType.GetDefaultConstructor();

            var properties = TargetType.GetAllProperties();
            var fields = TargetType.GetAllFields();
            _readers = new List<ReadElement>(fields.Length + properties.Length);

            // Gather the properties.
            foreach (var property in properties)
            {
                var read = GetElementReader(manager, property);
                if (read != null)
                    _readers.Add(read);
            }
            
            // Gather the fields.
            foreach (var field in fields)
            {
                var read = GetElementReader(manager, field);
                if (read != null)
                    _readers.Add(read);
            }
        }

        private static ReadElement? GetElementReader(ContentTypeReaderManager manager, MemberInfo member)
        {
            var property = member as PropertyInfo;
            var field = member as FieldInfo;
            Debug.Assert(field != null && property != null);

            if (property != null)
            {
                // Properties must have at least a getter.
                if (!property.CanRead)
                    return null;

                // Skip over indexer properties.
                if (property.GetIndexParameters().Any())
                    return null;
            }

            // Are we explicitly asked to ignore this item?
            if (member.GetCustomAttribute<ContentSerializerIgnoreAttribute>() != null) 
                return null;

            var contentSerializerAttribute = member.GetCustomAttribute<ContentSerializerAttribute>();
            if (contentSerializerAttribute == null)
            {
                if (property != null)
                {
                    // There is no ContentSerializerAttribute, so non-public
                    // properties cannot be deserialized.
                    if (!ReflectionHelpers.PropertyIsPublic(property))
                        return null;

                    // If the read-only property has a type reader,
                    // and CanDeserializeIntoExistingObject is true,
                    // then it is safe to deserialize into the existing object.
                    if (!property.CanWrite)
                    {
                        var typeReader = manager.GetTypeReader(property.PropertyType);
                        if (typeReader == null || !typeReader.CanDeserializeIntoExistingObject)
                            return null;
                    }
                }
                else
                {
                    // There is no ContentSerializerAttribute, so non-public
                    // fields cannot be deserialized.
                    if (!field.IsPublic)
                        return null;

                    // evolutional: Added check to skip initialise only fields
                    if (field.IsInitOnly)
                        return null;
                }
            }

            Type elementType;
            Action<object, object> setter;
            if (property != null)
            {
                elementType = property.PropertyType;
                if (property.CanWrite)
                    setter = (o, v) => property.SetValue(o, v, null);
                else
                    setter = (o, v) => { };
            }
            else
            {
                elementType = field.FieldType;
                setter = field.SetValue;
            }

            // Shared resources get special treatment.
            if (contentSerializerAttribute != null && contentSerializerAttribute.SharedResource)
            {
                return (input, parent) =>
                {
                    input.ReadSharedResource<object>((value) => setter(parent, value));
                };
            }

            // We need to have a reader at this point.
            var reader = manager.GetTypeReader(elementType);
            if (reader == null)
            {
                if (elementType == typeof(Array))
                    reader = new ArrayReader<Array>();
                else
                    throw new ContentLoadException(
                        $"Content reader could not be found for {elementType.FullName} type.");
            }

            // We use the construct delegate to pick the correct existing 
            // object to be the target of deserialization.
            Func<object, object?> construct = parent => null;

            if (property != null && !property.CanWrite)
                construct = parent => property.GetValue(parent, null);

            return (input, parent) =>
            {
                var existing = construct.Invoke(parent);
                var obj2 = input.ReadObject(reader, existing);
                setter(parent, obj2);
            };
        }
      
        protected internal override object? Read(ContentReader input, object? existingInstance)
        {
            T obj;
            if (existingInstance != null)
            {
                obj = (T) existingInstance;
            }
            else
            {
                obj = _constructor == null
                    ? Activator.CreateInstance<T>()
                    : (T) _constructor.Invoke(null);
            }
            _baseTypeReader?.Read(input, obj);

            Debug.Assert(_readers != null);

            var boxed = (object?)obj; // Box the type once.
            foreach (var reader in _readers)
                reader.Invoke(input, boxed);

            return obj;
        }
    }
}

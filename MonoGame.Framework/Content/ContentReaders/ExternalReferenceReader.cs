// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Content
{
    /// <summary>
    /// External reference reader, provided for compatibility with XNA Framework built content.
    /// </summary>
    internal class ExternalReferenceReader : ContentTypeReader
    {
        public ExternalReferenceReader() : base(typeof(object))
        {
        }

        protected internal override object? Read(ContentReader input, object? existingInstance)
        {
            return input.ReadExternalReference<object>();
        }
    }
}

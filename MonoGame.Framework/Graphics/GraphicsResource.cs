// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace MonoGame.Framework.Graphics
{
    public abstract class GraphicsResource : IDisposable
    {
        /// <summary>
        /// This field should only be accessed in <see cref="Dispose(bool)"/> if the disposing
        /// parameter is true. If disposing is false, this field may or may not be disposed yet.
        /// </summary>
        private GraphicsDevice? _graphicsDevice;

        private WeakReference? _selfReference;

        /// <summary>
        /// Occurs when the <see cref="GraphicsResource"/> is disposed.
        /// </summary>
        public event Event<GraphicsResource>? Disposing;

        /// <summary>
        /// Gets whether the <see cref="GraphicsResource"/> is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets the name of this <see cref="GraphicsResource"/>.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the tag object of this <see cref="GraphicsResource"/>.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Gets whether graphics operations are supported when not called on the main thread.
        /// </summary>
        public virtual bool SupportsAsync => GraphicsDevice.Capabilities.SupportsAsync;

        /// <summary>
        /// Gets whether the caller is on the main thread or 
        /// whether graphics operations are supported when not called on the main thread.
        /// </summary>
        protected bool IsValidThreadContext => Threading.IsOnMainThread || SupportsAsync;

        protected virtual bool IsDefaultStateObject => false;

        /// <summary>
        /// Gets the <see cref="Graphics.GraphicsDevice"/> assigned to this <see cref="GraphicsResource"/>.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                Debug.Assert(_graphicsDevice != null);
                return _graphicsDevice;
            }
            internal set
            {
                ArgumentNullException.ThrowIfNull(value);
                if (_graphicsDevice == value)
                    return;

                // VertexDeclaration objects can be bound to multiple GraphicsDevice objects
                // during their lifetime. But only one GraphicsDevice should retain ownership.
                if (_graphicsDevice != null)
                {
                    _graphicsDevice.RemoveResourceReference(_selfReference);
                    _selfReference = null;
                }
                _graphicsDevice = value;

                _selfReference = new WeakReference(this);
                _graphicsDevice.AddResourceReference(_selfReference);
            }
        }

        internal GraphicsResource(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(
                nameof(graphicsDevice), FrameworkResources.ResourceCreationWithNullDevice);
        }

        internal GraphicsResource()
        {
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (IsDefaultStateObject)
            {
                Throw();
                void Throw() => ThrowHelper.InvalidOperation(
                    $"You cannot bind a default {GetType().Name} object.");
            }

            if (_graphicsDevice != null && _graphicsDevice != device)
            {
                Throw();
                void Throw() => ThrowHelper.InvalidOperation(
                    $"This {GetType().Name} is already bound to a different graphics device.");
            }

            GraphicsDevice = device;
        }

        internal void ThrowIfBound()
        {
            if (IsDefaultStateObject)
            {
                Throw();
                void Throw() => ThrowHelper.InvalidOperation(
                    $"You cannot modify a default {GetType().Name} object.");
            }

            if (_graphicsDevice != null)
            {
                Throw();
                void Throw() => ThrowHelper.InvalidOperation(
                    $"You cannot modify {GetType().Name} after it has been bound to a graphics device.");
            }
        }

        internal void InvokeGraphicsDeviceResetting()
        {
            GraphicsDeviceResetting();
        }

        /// <summary>
        /// Called before the device is reset. Allows graphics resources to 
        /// invalidate their state so they can be recreated after the device resets.
        /// </summary>
        /// <remarks>
        /// This may be called after <see cref="Dispose()"/> up until the resource is garbage collected.
        /// </remarks>
        protected virtual void GraphicsDeviceResetting()
        {
        }

        /// <summary>
        /// Throws an exception if the caller is not running on the main thread
        /// and the resource does not support asynchronous operations.
        /// </summary>
        /// <exception cref="OffThreadNotSupportedException">
        /// The caller is not on the main thread.
        /// </exception>
        protected void AssertMainThread(bool isSpanOverload)
        {
            if (SupportsAsync)
                return;

            if (!Threading.IsOnMainThread)
            {
                var msg = isSpanOverload ? FrameworkResources.OffThreadSpanNotSupported : null;
                ThrowHelper.OffThreadNotSupported(msg);
            }
        }

        /// <summary>
        /// Returns the string representation of this <see cref="GraphicsResource"/>.
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + (string.IsNullOrEmpty(Name) ? "" : (": \"" + Name + "\""));
        }

        /// <summary>
        /// The method that derived classes should override to implement disposing of managed and native resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed objects should be disposed.</param>
        /// <remarks>
        /// Unmanaged resources should always be released regardless of the value of the disposing parameter.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            // Do not trigger the event if called from the finalizer
            if (disposing)
                Disposing?.Invoke(this);

            // Remove from the global list of graphics resources
            _graphicsDevice?.RemoveResourceReference(_selfReference);
            _graphicsDevice = null;
            _selfReference = null;

            IsDisposed = true;
        }

        /// <summary>
        /// Releases resources used by the <see cref="GraphicsResource"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the <see cref="GraphicsResource"/>.
        /// </summary>
        ~GraphicsResource()
        {
            Dispose(false);
        }
    }
}


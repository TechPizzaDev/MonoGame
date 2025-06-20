// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Graphics
{
    /// <summary>
    /// Provider of a <see cref="Graphics.GraphicsDevice"/>.
    /// </summary>
    public interface IGraphicsDeviceService
    {
        /// <summary>
        /// The provided <see cref="Graphics.GraphicsDevice"/>.
        /// </summary>
        GraphicsDevice? GraphicsDevice { get; }

        /// <summary>
        /// Raised when a new <see cref="Graphics.GraphicsDevice"/> has been created.
        /// </summary>
        event Event<IGraphicsDeviceService>? DeviceCreated;

        /// <summary>
        /// Raised when the <see cref="GraphicsDevice"/> is disposed.
        /// </summary>
        event Event<IGraphicsDeviceService>? DeviceDisposing;

        /// <summary>
        /// Raised when the <see cref="GraphicsDevice"/> has reset.
        /// </summary>
        /// <seealso cref="GraphicsDevice.Reset()"/>
        event Event<IGraphicsDeviceService>? DeviceReset;

        /// <summary>
        /// Raised before the <see cref="GraphicsDevice"/> is resetting.
        /// </summary>
        event Event<IGraphicsDeviceService>? DeviceResetting;
    }
}


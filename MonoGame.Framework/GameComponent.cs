// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework
{
    /// <summary>
    /// An object that can be attached to a <see cref="Game"/> and have its <see cref="Update"/>
    /// method called when <see cref="Game.Update"/> is called.
    /// </summary>
    public class GameComponent : IComparable<GameComponent>, IGameComponent, IUpdateable, IDisposable
    {
        private bool _enabled = true;
        private int _updateOrder;

        /// <summary>
        /// The <see cref="Game"/> that owns this <see cref="GameComponent"/>.
        /// </summary>
        public Game Game { get; private set; }

        /// <inheritdoc />
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        /// <inheritdoc />
        public int UpdateOrder
        {
            get => _updateOrder;
            set
            {
                if (_updateOrder != value)
                {
                    _updateOrder = value;
                    OnUpdateOrderChanged();
                }
            }
        }

        /// <inheritdoc />
        public event Event<IUpdateable>? EnabledChanged;

        /// <inheritdoc />
        public event Event<IUpdateable>? UpdateOrderChanged;

        public GameComponent(Game game)
        {
            Game = game;
        }

        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Update the component.
        /// </summary>
        /// <param name="time"><see cref="FrameTime"/> of the <see cref="Game"/>.</param>
        public virtual void Update(in FrameTime time)
        {
        }

        /// <summary>
        /// Called when <see cref="UpdateOrder"/> changed. Raises the <see cref="UpdateOrderChanged"/> event.
        /// </summary>
        protected virtual void OnUpdateOrderChanged()
        {
            UpdateOrderChanged?.Invoke(this);
        }

        /// <summary>
        /// Called when <see cref="Enabled"/> changed. Raises the <see cref="EnabledChanged"/> event.
        /// </summary>
        protected virtual void OnEnabledChanged()
        {
            EnabledChanged?.Invoke(this);
        }

        #region IComparable<GameComponent> Members

        // TODO: Should be removed, as it is not part of XNA 4.0
        public int CompareTo(GameComponent? other)
        {
            if (this == other)
                return 0;
            if (other == null)
                return 1;

            return other.UpdateOrder - UpdateOrder;
        }

        #endregion

        /// <summary>
        /// Shuts down the component.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Shuts down the component.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

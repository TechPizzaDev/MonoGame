// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Input
{
    /// <summary>
    /// Contains information about the left and the right trigger buttons.
    /// </summary>
    public readonly struct GamePadTriggers : IEquatable<GamePadTriggers>
    {
        /// <summary>
        /// Gets the position of the left trigger.
        /// </summary>
        /// <value>A value from 0f to 1f representing left trigger.</value>
        public float Left { get; }

        /// <summary>
        /// Gets the position of the right trigger.
        /// </summary>
        /// <value>A value from 0f to 1f representing right trigger.</value>
        public float Right { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePadTriggers"/> struct.
        /// </summary>
        /// <param name="leftTrigger">The position of the left trigger, the value will get clamped between 0f and 1f.</param>
        /// <param name="rightTrigger">The position of the right trigger, the value will get clamped between 0f and 1f.</param>
        public GamePadTriggers(float leftTrigger, float rightTrigger)
        {
            Left = Math.Clamp(leftTrigger, 0f, 1f);
            Right = Math.Clamp(rightTrigger, 0f, 1f);
        }

        #region Equals

        public static bool operator ==(in GamePadTriggers a, in GamePadTriggers b) => 
            (a.Left == b.Left) && (a.Right == b.Right);

        public static bool operator !=(in GamePadTriggers a, in GamePadTriggers b) => !(a == b);

        public bool Equals(GamePadTriggers other) => this == other;
        public override bool Equals(object? obj) => obj is GamePadTriggers other && Equals(other);

        #endregion

        /// <summary>
        /// Serves as a hash function for a <see cref="GamePadTriggers"/> object.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(Left, Right);

        /// <summary>
        /// Returns a string that represents the current <see cref="GamePadTriggers"/>.
        /// </summary>
        public override string ToString() => "[GamePadTriggers: Left=" + Left + ", Right=" + Right + "]";
    }
}

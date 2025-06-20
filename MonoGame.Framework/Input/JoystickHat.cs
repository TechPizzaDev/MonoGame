// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Input
{
    /// <summary>
    /// Describes joystick hat state.
    /// </summary>
    public readonly struct JoystickHat : IEquatable<JoystickHat>
    {
        /// <summary>
        /// Gets if joysticks hat "down" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Down { get; }

        /// <summary>
        /// Gets if joysticks hat "left" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Left { get; }

        /// <summary>
        /// Gets if joysticks hat "right" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Right { get; }

        /// <summary>
        /// Gets if joysticks hat "up" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Up { get; }

        /// <summary>
        /// Constructs the <see cref="JoystickHat"/> with the given button states.
        /// </summary>
        public JoystickHat(ButtonState down, ButtonState left, ButtonState right, ButtonState up)
        {
            Down = down;
            Left = left;
            Right = right;
            Up = up;
        }

        #region Equals

        public static bool operator ==(JoystickHat a, JoystickHat b)
        {
            return (a.Down == b.Down)
                && (a.Left == b.Left)
                && (a.Right == b.Right)
                && (a.Up == b.Up);
        }

        public static bool operator !=(JoystickHat a, JoystickHat b) => !(a == b);

        public bool Equals(JoystickHat other) => this == other;
        public override bool Equals(object? obj) => obj is JoystickHat other && Equals(other);

        #endregion

        /// <summary>
        /// Returns the hash code of the <see cref="JoystickHat"/>.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = 0;
            if (Left == ButtonState.Pressed) hash |= 1 << 3;
            if (Up == ButtonState.Pressed) hash |= 1 << 2;
            if (Right == ButtonState.Pressed) hash |= 1 << 1;
            if (Down == ButtonState.Pressed) hash |= 1 << 0;
            return hash;
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="JoystickHat"/> in a 
        /// format of 0000 where each number represents a boolean value of each respecting property:
        /// <see cref="Left"/>, <see cref="Up"/>, <see cref="Right"/>, <see cref="Down"/>.
        /// </summary>
        public override string ToString() => string.Concat(
                ((int)Left).ToString(),
                ((int)Up).ToString(),
                ((int)Right).ToString(),
                ((int)Down).ToString());
    }
}


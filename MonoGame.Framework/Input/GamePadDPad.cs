// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Input
{
    public readonly struct GamePadDPad : IEquatable<GamePadDPad>
    {
        private readonly ButtonState _down;
        private readonly ButtonState _left;
        private readonly ButtonState _right;
        private readonly ButtonState _up;

        /// <summary>
        /// Gets a value indicating wethever down is pressed on the directional pad.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the down button is pressed; otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Down => _down;

        /// <summary>
        /// Gets a value indicating wethever left is pressed on the directional pad.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the left button is pressed; otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Left => _left;

        /// <summary>
        /// Gets a value indicating wethever right is pressed on the directional pad.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the right button is pressed; otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Right => _right;

        /// <summary>
        /// Gets a value indicating wethever up is pressed on the directional pad.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the up button is pressed; otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Up => _up;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePadDPad"/> struct.
        /// </summary>
        /// <param name="upValue">Current state of directional up pad.</param>
        /// <param name="downValue">Current state of directional down pad.</param>
        /// <param name="leftValue">Current state of directional left pad.</param>
        /// <param name="rightValue">Current state of directional right pad.</param>
        public GamePadDPad(
            ButtonState upValue, ButtonState downValue, ButtonState leftValue, ButtonState rightValue) : this()
        {
            _up = upValue;
            _down = downValue;
            _left = leftValue;
            _right = rightValue;
        }

        public GamePadDPad(ReadOnlySpan<Buttons> buttons) : this()
        {
            foreach (var button in buttons)
                ConvertButtonToDirection(button, ref _down, ref _left, ref _right, ref _up);
        }

        internal GamePadDPad(Buttons button) : this()
        {
            ConvertButtonToDirection(button, ref _down, ref _left, ref _right, ref _up);
        }

        #endregion

        private void ConvertButtonToDirection(Buttons button,
            ref ButtonState down, ref ButtonState left,
            ref ButtonState right, ref ButtonState up)
        {
            if ((button & Buttons.DPadDown) == Buttons.DPadDown)
                down = ButtonState.Pressed;

            if ((button & Buttons.DPadLeft) == Buttons.DPadLeft)
                left = ButtonState.Pressed;

            if ((button & Buttons.DPadRight) == Buttons.DPadRight)
                right = ButtonState.Pressed;

            if ((button & Buttons.DPadUp) == Buttons.DPadUp)
                up = ButtonState.Pressed;
        }

        #region Equals

        public static bool operator ==(in GamePadDPad left, in GamePadDPad right)
        {
            return (left.Down == right.Down)
                && (left.Left == right.Left)
                && (left.Right == right.Right)
                && (left.Up == right.Up);
        }

        public static bool operator !=(in GamePadDPad left, in GamePadDPad right) => !(left == right);

        public bool Equals(GamePadDPad other) => this == other;
        public override bool Equals(object? obj) => obj is GamePadDPad other && Equals(other);

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns the hash code of the <see cref="GamePadDPad"/>.
        /// </summary>
        public override int GetHashCode()
        {
            return
                ((Down == ButtonState.Pressed) ? 1 : 2) +
                ((Left == ButtonState.Pressed) ? 4 : 8) +
                ((Right == ButtonState.Pressed) ? 16 : 32) +
                ((Up == ButtonState.Pressed) ? 64 : 128);
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="GamePadDPad"/>
        /// in a format of 0000 where each number represents a boolean value of each 
        /// respective property: <see cref="Left"/>, <see cref="Up"/>, <see cref="Right"/>, <see cref="Down"/>.
        /// </summary>
        public override string ToString()
        {
            return string.Concat(
                ((int)Left).ToString(),
                ((int)Up).ToString(),
                ((int)Right).ToString(),
                ((int)Down).ToString());
        }

        #endregion
    }
}

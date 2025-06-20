// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Input
{
    /// <summary>
    /// Represents the current button states for the controller.
    /// </summary>
    public readonly struct GamePadButtons : IEquatable<GamePadButtons>
    {
        internal readonly Buttons _buttons;

        /// <summary>
        /// Gets a value indicating whether the button A is pressed.
        /// </summary>
        public ButtonState A => 
            ((_buttons & Buttons.A) == Buttons.A) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the button B is pressed.
        /// </summary>
        public ButtonState B => 
            ((_buttons & Buttons.B) == Buttons.B) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the button Back is pressed.
        /// </summary>
        public ButtonState Back => 
            ((_buttons & Buttons.Back) == Buttons.Back) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the button X is pressed.
        /// </summary>
        public ButtonState X => 
            ((_buttons & Buttons.X) == Buttons.X) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the button Y is pressed.
        /// </summary>
        public ButtonState Y => 
            ((_buttons & Buttons.Y) == Buttons.Y) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the button Start is pressed.
        /// </summary>
        public ButtonState Start => 
            ((_buttons & Buttons.Start) == Buttons.Start) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the left shoulder button is pressed.
        /// </summary>
        public ButtonState LeftShoulder => 
            ((_buttons & Buttons.LeftShoulder) == Buttons.LeftShoulder) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the left stick button is pressed.
        /// </summary>
        public ButtonState LeftStick => 
            ((_buttons & Buttons.LeftStick) == Buttons.LeftStick) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the right shoulder button is pressed.
        /// </summary>
        public ButtonState RightShoulder => 
            ((_buttons & Buttons.RightShoulder) == Buttons.RightShoulder) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the right stick button is pressed.
        /// </summary>
        public ButtonState RightStick => 
            ((_buttons & Buttons.RightStick) == Buttons.RightStick) ? ButtonState.Pressed : ButtonState.Released;

        /// <summary>
        /// Gets a value indicating whether the guide button is pressed.
        /// </summary>
        public ButtonState BigButton => 
            ((_buttons & Buttons.BigButton) == Buttons.BigButton) ? ButtonState.Pressed : ButtonState.Released;

        public GamePadButtons(Buttons buttons)
        {
            _buttons = buttons;
        }

        internal GamePadButtons(ReadOnlySpan<Buttons> buttons) : this()
        {
            foreach (Buttons b in buttons)
                _buttons |= b;
        }

        #region Equals

        public static bool operator ==(GamePadButtons a, GamePadButtons b) => a._buttons == b._buttons;
        public static bool operator !=(GamePadButtons a, GamePadButtons b) => a._buttons != b._buttons;

        public bool Equals(GamePadButtons other) => this == other;
        public override bool Equals(object? obj) => obj is GamePadButtons other && Equals(other);

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns the hash code of the <see cref="GamePadButtons"/>.
        /// </summary>
        public override int GetHashCode() => _buttons.GetHashCode();

        /// <summary>
        /// Returns a string that represents the <see cref="GamePadButtons"/>.
        /// </summary>
        public override string ToString()
        {
            return
                "{A=" + (int)A +
                ", B=" + (int)B +
                ", Back=" + (int)Back +
                ", X=" + (int)X +
                ", Y=" + (int)Y +
                ", Start=" + (int)Start +
                ", LeftShoulder=" + (int)LeftShoulder +
                ", LeftStick=" + (int)LeftStick +
                ", RightShoulder=" + (int)RightShoulder +
                ", RightStick=" + (int)RightStick +
                ", BigButton=" + (int)BigButton +
                "}";
        }

        #endregion
    }
}


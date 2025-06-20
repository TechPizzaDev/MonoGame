// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Text;

namespace MonoGame.Framework.Input
{
    /// <summary>
    /// Describes a joystick state.
    /// </summary>
    public readonly struct JoystickState : IEquatable<JoystickState>
    {
        /// <summary>
        /// The default <see cref="JoystickState"/>.
        /// </summary>
        public static JoystickState Default { get; } = new JoystickState(
            isConnected: false,
            axes: Array.Empty<int>(),
            buttons: Array.Empty<ButtonState>(),
            hats: Array.Empty<JoystickHat>());

        /// <summary>
        /// Gets a value indicating whether the joystick is connected.
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// Gets the joystick axis values.
        /// </summary>
        public ReadOnlyMemory<int> Axes { get; }

        /// <summary>
        /// Gets the joystick button values.
        /// </summary>
        public ReadOnlyMemory<ButtonState> Buttons { get; }

        /// <summary>
        /// Gets the joystick hat values.
        /// </summary>
        public ReadOnlyMemory<JoystickHat> Hats { get; }

        /// <summary>
        /// Constructs the <see cref="JoystickState"/>.
        /// </summary>
        public JoystickState(
            bool isConnected,
            ReadOnlyMemory<int> axes, 
            ReadOnlyMemory<ButtonState> buttons, 
            ReadOnlyMemory<JoystickHat> hats)
        {
            IsConnected = isConnected;
            Axes = !axes.IsEmpty ? axes : throw new ArgumentEmptyException(nameof(axes));
            Buttons = !buttons.IsEmpty ? buttons : throw new ArgumentEmptyException(nameof(buttons));
            Hats = !hats.IsEmpty ? hats : throw new ArgumentEmptyException(nameof(hats));
        }

        #region Equals

        public static bool operator ==(in JoystickState a, in JoystickState b)
        {
            var aButtons = a.Buttons.Span;
            var bButtons = b.Buttons.Span;
            if (aButtons.Length != bButtons.Length)
                return false;

            for (int i = 0; i < aButtons.Length; i++)
                if (aButtons[i] != bButtons[i])
                    return false;

            return a.IsConnected == b.IsConnected
                && a.Axes.Span.SequenceEqual(b.Axes.Span)
                && a.Hats.Span.SequenceEqual(b.Hats.Span);
        }

        public static bool operator !=(in JoystickState a, in JoystickState b) => !(a == b);

        public bool Equals(JoystickState other) => this == other;
        public override bool Equals(object? obj) => obj is JoystickState other && Equals(other);

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns the hash code of the <see cref="JoystickState"/>.
        /// </summary>
        public override int GetHashCode()
        {
            if (!IsConnected)
                return 0;

            unchecked
            {
                int code = 7;
                foreach (int axis in Axes.Span)
                    code = code * 31 + axis;

                foreach (var button in Buttons.Span)
                    code = code * 31 + button.GetHashCode();

                foreach (var hat in Hats.Span)
                    code = code * 31 + hat.GetHashCode();
                return code;
            }
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="JoystickState"/>.
        /// </summary>
        public override string ToString()
        {
            var ret = new StringBuilder(
                58 +
                Axes.Length * 7 +
                Buttons.Length +
                Hats.Length * 5);

            ret.Append("[JoystickState: IsConnected=");
            ret.Append(IsConnected);

            if (IsConnected)
            {
                if (!Axes.IsEmpty)
                {
                    ret.Append(", Axes=");
                    foreach (var axis in Axes.Span)
                        ret.Append((axis > 0 ? "+" : "") + axis.ToString("00000") + " ");
                    ret.Length--;
                }

                if (!Buttons.IsEmpty)
                {
                    ret.Append(", Buttons=");
                    foreach (var button in Buttons.Span)
                        ret.Append((int)button);
                }

                if (!Hats.IsEmpty)
                {
                    ret.Append(", Hats=");
                    foreach (var hat in Hats.Span)
                        ret.Append(hat + " ");
                    ret.Length--;
                }
            }

            ret.Append(']');
            return ret.ToString();
        }

        #endregion
    }
}


// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;

namespace MonoGame.Framework.Input
{
    /// <summary>
    /// Represents the current thumbstick states for the controller.
    /// </summary>
    public readonly struct GamePadThumbSticks : IEquatable<GamePadThumbSticks>
    {
#if DIRECTX && !WINDOWS_UAP
        // XInput Xbox 360 Controller dead zones
        // Dead zones are slighty different between left and right sticks, this may come from Microsoft usability tests
        private const float LeftThumbDeadZone = SharpDX.XInput.Gamepad.LeftThumbDeadZone / (float)short.MaxValue;
        private const float RightThumbDeadZone = SharpDX.XInput.Gamepad.RightThumbDeadZone / (float)short.MaxValue;
#else
        // Default & SDL Xbox 360 Controller dead zones based on the XInput constants.
        private const float LeftThumbDeadZone = 0.24f;
        private const float RightThumbDeadZone = 0.265f;
#endif

        internal readonly Buttons _virtualButtons;

        /// <summary>
        /// Gets a value indicating the position of the left stick (thumbstick). 
        /// </summary>
        /// <value>A <see cref="Vector2"/> indicating the current position of the left stick (thumbstick).</value>
        public Vector2 Left { get; }

        /// <summary>
        /// Gets a value indicating the position of the right stick (thumbstick). 
        /// </summary>
        /// <value>A <see cref="Vector2"/> indicating the current position of the right stick (thumbstick).</value>
        public Vector2 Right { get; }

        public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition)
            : this(leftPosition, rightPosition, GamePadDeadZone.None, GamePadDeadZone.None)
        {
        }

        internal GamePadThumbSticks(
            Vector2 leftPosition, Vector2 rightPosition,
            GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode) : this()
        {
            // Apply dead zone
            Left = ApplyDeadZone(leftDeadZoneMode, LeftThumbDeadZone, leftPosition);
            Right = ApplyDeadZone(rightDeadZoneMode, RightThumbDeadZone, rightPosition);

            // VirtualButtons should always behave like deadzone is IndependentAxes. 
            // This is consistent with XNA behaviour and generally most convenient (e.g. for menu navigation)
            _virtualButtons = 0;

            if (leftPosition.X < -LeftThumbDeadZone) _virtualButtons |= Buttons.LeftThumbstickLeft;
            else if (leftPosition.X > LeftThumbDeadZone) _virtualButtons |= Buttons.LeftThumbstickRight;

            if (leftPosition.Y < -LeftThumbDeadZone) _virtualButtons |= Buttons.LeftThumbstickDown;
            else if (leftPosition.Y > LeftThumbDeadZone) _virtualButtons |= Buttons.LeftThumbstickUp;

            if (rightPosition.X < -RightThumbDeadZone) _virtualButtons |= Buttons.RightThumbstickLeft;
            else if (rightPosition.X > RightThumbDeadZone) _virtualButtons |= Buttons.RightThumbstickRight;

            if (rightPosition.Y < -RightThumbDeadZone) _virtualButtons |= Buttons.RightThumbstickDown;
            else if (rightPosition.Y > RightThumbDeadZone) _virtualButtons |= Buttons.RightThumbstickUp;
        }

        private Vector2 ApplyDeadZone(GamePadDeadZone deadZoneMode, float deadZone, Vector2 thumbstickPosition)
        {
            // XNA applies dead zones before rounding/clamping values. 
            // The public ctor does not allow this because the dead zone must be known before

            // Apply dead zone
            switch (deadZoneMode)
            {
                case GamePadDeadZone.None:
                    break;

                case GamePadDeadZone.IndependentAxes:
                    thumbstickPosition = ExcludeIndependentAxesDeadZone(thumbstickPosition, deadZone);
                    break;

                case GamePadDeadZone.Circular:
                    thumbstickPosition = ExcludeCircularDeadZone(thumbstickPosition, deadZone);
                    break;
            }

            // Apply clamp
            if (deadZoneMode == GamePadDeadZone.Circular)
            {
                if (thumbstickPosition.LengthSquared() > 1f)
                    thumbstickPosition = Vector2.Normalize(thumbstickPosition);
            }
            else
            {
                thumbstickPosition = VectorHelper.Clamp(thumbstickPosition, -1, 1);
            }

            return thumbstickPosition;
        }

        private Vector2 ExcludeIndependentAxesDeadZone(Vector2 value, float deadZone)
        {
            return new Vector2(
                ExcludeAxisDeadZone(value.X, deadZone),
                ExcludeAxisDeadZone(value.Y, deadZone));
        }

        private float ExcludeAxisDeadZone(float value, float deadZone)
        {
            if (value < -deadZone)
                value += deadZone;
            else if (value > deadZone)
                value -= deadZone;
            else
                return 0f;
            return value / (1f - deadZone);
        }

        private Vector2 ExcludeCircularDeadZone(Vector2 value, float deadZone)
        {
            float originalLength = value.Length();
            if (originalLength <= deadZone)
                return Vector2.Zero;

            float newLength = (originalLength - deadZone) / (1f - deadZone);
            return value * (newLength / originalLength);
        }

        #region Equals

        public static bool operator ==(in GamePadThumbSticks a, in GamePadThumbSticks b) =>
            a.Left == b.Left && a.Right == b.Right;

        public static bool operator !=(in GamePadThumbSticks a, in GamePadThumbSticks b) => !(a == b);

        public bool Equals(GamePadThumbSticks other) => this == other;
        public override bool Equals(object? obj) => obj is GamePadThumbSticks other && Equals(other);

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns the hash code of the <see cref="GamePadThumbSticks"/>.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(Left, Right);

        /// <summary>
        /// Returns a string that represents the current <see cref="GamePadThumbSticks"/>.
        /// </summary>
        public override string ToString() => "{Left=" + Left + ", Right=" + Right + "}";

        #endregion
    }
}
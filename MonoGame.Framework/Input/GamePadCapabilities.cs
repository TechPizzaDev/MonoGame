// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Input
{
    /// <summary>
    /// Represents a controller's capabilities.
    /// </summary>
    public readonly struct GamePadCapabilities : IEquatable<GamePadCapabilities>
    {
        #region Fields

        /// <summary>
        /// Gets a value indicating whether the controller is connected.
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// Gets the gamepad display name.
        /// This property is not available in XNA.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Gets the unique identifier of the gamepad.
        /// This property is not available in XNA.
        /// </summary>
        public string? Identifier { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button A.
        /// </summary>
        public bool HasAButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Back.
        /// </summary>
        public bool HasBackButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button B.
        /// </summary>
        public bool HasBButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad down button.
        /// </summary>
        public bool HasDPadDownButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad left button.
        /// </summary>
        public bool HasDPadLeftButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad right button.
        /// </summary>
        public bool HasDPadRightButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad up button.
        /// </summary>
        public bool HasDPadUpButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the left shoulder button.
        /// </summary>
        public bool HasLeftShoulderButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the left stick button.
        /// </summary>
        public bool HasLeftStickButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the right shoulder button.
        /// </summary>
        public bool HasRightShoulderButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the right stick button.
        /// </summary>
        public bool HasRightStickButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Start.
        /// </summary>
        public bool HasStartButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button X.
        /// </summary>
        public bool HasXButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Y.
        /// </summary>
        public bool HasYButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the guide button.
        /// </summary>
        public bool HasBigButton { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has X axis for the left stick (thumbstick) button.
        /// </summary>
        public bool HasLeftXThumbStick { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has Y axis for the left stick (thumbstick) button.
        /// </summary>
        public bool HasLeftYThumbStick { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has X axis for the right stick (thumbstick) button.
        /// </summary>
        public bool HasRightXThumbStick { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has Y axis for the right stick (thumbstick) button.
        /// </summary>
        public bool HasRightYThumbStick { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the left trigger button.
        /// </summary>
        public bool HasLeftTrigger { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the right trigger button.
        /// </summary>
        public bool HasRightTrigger { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the left vibration motor.
        /// </summary>
        public bool HasLeftVibrationMotor { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has the right vibration motor.
        /// </summary>
        public bool HasRightVibrationMotor { get; }

        /// <summary>
        /// Gets a value indicating whether the controller has a microphone.
        /// </summary>
        public bool HasVoiceSupport { get; }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        public GamePadType GamePadType { get; }

        #endregion

        /// <summary>
        /// Constructs the <see cref="GamePadCapabilities"/> with optional parameters.
        /// </summary>
        public GamePadCapabilities(
            bool isConnected = false, 
            string? displayName = null, 
            string? identifier = null,
            bool hasAButton = false, 
            bool hasBButton = false,
            bool hasBackButton = false,
            bool hasDPadDownButton = false,
            bool hasDPadLeftButton = false, 
            bool hasDPadRightButton = false,
            bool hasDPadUpButton = false,
            bool hasLeftShoulderButton = false,
            bool hasLeftStickButton = false,
            bool hasRightShoulderButton = false,
            bool hasRightStickButton = false,
            bool hasStartButton = false,
            bool hasXButton = false, 
            bool hasYButton = false,
            bool hasBigButton = false,
            bool hasLeftXThumbStick = false,
            bool hasLeftYThumbStick = false, 
            bool hasRightXThumbStick = false,
            bool hasRightYThumbStick = false,
            bool hasLeftTrigger = false,
            bool hasRightTrigger = false,
            bool hasLeftVibrationMotor = false,
            bool hasRightVibrationMotor = false,
            bool hasVoiceSupport = false,
            GamePadType gamePadType = GamePadType.Unknown)
        {
            IsConnected = isConnected;
            DisplayName = displayName;
            Identifier = identifier;
            HasAButton = hasAButton;
            HasBButton = hasBButton;
            HasBackButton = hasBackButton;
            HasDPadDownButton = hasDPadDownButton;
            HasDPadLeftButton = hasDPadLeftButton;
            HasDPadRightButton = hasDPadRightButton;
            HasDPadUpButton = hasDPadUpButton;
            HasLeftShoulderButton = hasLeftShoulderButton;
            HasLeftStickButton = hasLeftStickButton;
            HasRightShoulderButton = hasRightShoulderButton;
            HasRightStickButton = hasRightStickButton;
            HasStartButton = hasStartButton;
            HasXButton = hasXButton;
            HasYButton = hasYButton;
            HasBigButton = hasBigButton;
            HasLeftXThumbStick = hasLeftXThumbStick;
            HasLeftYThumbStick = hasLeftYThumbStick;
            HasRightXThumbStick = hasRightXThumbStick;
            HasRightYThumbStick = hasRightYThumbStick;
            HasLeftTrigger = hasLeftTrigger;
            HasRightTrigger = hasRightTrigger;
            HasLeftVibrationMotor = hasLeftVibrationMotor;
            HasRightVibrationMotor = hasRightVibrationMotor;
            HasVoiceSupport = hasVoiceSupport;
            GamePadType = gamePadType;
        }

        #region Equals

        public static bool operator ==(in GamePadCapabilities left, in GamePadCapabilities right)
        {
            return left.DisplayName == right.DisplayName
                && left.Identifier == right.Identifier
                && left.IsConnected == right.IsConnected
                && left.HasAButton == right.HasAButton
                && left.HasBackButton == right.HasBackButton
                && left.HasBButton == right.HasBButton
                && left.HasDPadDownButton == right.HasDPadDownButton
                && left.HasDPadLeftButton == right.HasDPadLeftButton
                && left.HasDPadRightButton == right.HasDPadRightButton
                && left.HasDPadUpButton == right.HasDPadUpButton
                && left.HasLeftShoulderButton == right.HasLeftShoulderButton
                && left.HasLeftStickButton == right.HasLeftStickButton
                && left.HasRightShoulderButton == right.HasRightShoulderButton
                && left.HasRightStickButton == right.HasRightStickButton
                && left.HasStartButton == right.HasStartButton
                && left.HasXButton == right.HasXButton
                && left.HasYButton == right.HasYButton
                && left.HasBigButton == right.HasBigButton
                && left.HasLeftXThumbStick == right.HasLeftXThumbStick
                && left.HasLeftYThumbStick == right.HasLeftYThumbStick
                && left.HasRightXThumbStick == right.HasRightXThumbStick
                && left.HasRightYThumbStick == right.HasRightYThumbStick
                && left.HasLeftTrigger == right.HasLeftTrigger
                && left.HasRightTrigger == right.HasRightTrigger
                && left.HasLeftVibrationMotor == right.HasLeftVibrationMotor
                && left.HasRightVibrationMotor == right.HasRightVibrationMotor
                && left.HasVoiceSupport == right.HasVoiceSupport
                && left.GamePadType == right.GamePadType;
        }

        public static bool operator !=(in GamePadCapabilities a, in GamePadCapabilities b)
        {
            return !(a == b);
        }

        public bool Equals(GamePadCapabilities other)
        {
            return this == other;
        }

        public override bool Equals(object? obj)
        {
            return obj is GamePadCapabilities other && this == other;
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Returns the hash code of the <see cref="GamePadCapabilities"/>.
        /// </summary>
        public override int GetHashCode() => Identifier?.GetHashCode() ?? DisplayName?.GetHashCode() ?? 0;

        /// <summary>
        /// Returns a string that represents the current <see cref="GamePadCapabilities"/>.
        /// </summary>
        public override string ToString()
        {
            return 
                "{IsConnected=" + IsConnected +
                ", DisplayName=" + DisplayName +
                ", Identifier=" + Identifier +
                ", HasAButton=" + HasAButton +
                ", HasBackButton=" + HasBackButton +
                ", HasBButton=" + HasBButton +
                ", HasDPadDownButton=" + HasDPadDownButton +
                ", HasDPadLeftButton=" + HasDPadLeftButton +
                ", HasDPadRightButton=" + HasDPadRightButton +
                ", HasDPadUpButton=" + HasDPadUpButton +
                ", HasLeftShoulderButton=" + HasLeftShoulderButton +
                ", HasLeftStickButton=" + HasLeftStickButton +
                ", HasRightShoulderButton=" + HasRightShoulderButton +
                ", HasRightStickButton=" + HasRightStickButton +
                ", HasStartButton=" + HasStartButton +
                ", HasXButton=" + HasXButton +
                ", HasYButton=" + HasYButton +
                ", HasBigButton=" + HasBigButton +
                ", HasLeftXThumbStick=" + HasLeftXThumbStick +
                ", HasLeftYThumbStick=" + HasLeftYThumbStick +
                ", HasRightXThumbStick=" + HasRightXThumbStick +
                ", HasRightYThumbStick=" + HasRightYThumbStick +
                ", HasLeftTrigger=" + HasLeftTrigger +
                ", HasRightTrigger=" + HasRightTrigger +
                ", HasLeftVibrationMotor=" + HasLeftVibrationMotor +
                ", HasRightVibrationMotor=" + HasRightVibrationMotor +
                ", HasVoiceSupport=" + HasVoiceSupport +
                ", GamePadType=" + GamePadType +
                "}";
        }

        #endregion
    }
}

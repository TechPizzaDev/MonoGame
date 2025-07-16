// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Numerics;
using SharpDX.XInput;
using GBF = SharpDX.XInput.GamepadButtonFlags;

namespace MonoGame.Framework.Input
{
    static partial class GamePad
    {
        internal static bool Back;

        private static readonly Controller[] _controllers = new[]
        {
            new Controller(UserIndex.One),
            new Controller(UserIndex.Two),
            new Controller(UserIndex.Three),
            new Controller(UserIndex.Four),
        };

        private static readonly bool[] _connected = new bool[4];
        private static readonly long[] _timeout = new long[4];
        private static readonly long TimeoutTicks = TimeSpan.FromSeconds(1).Ticks;

        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 4;
        }

        private static GamePadType DeviceSubTypeToGamePadType(DeviceSubType subType)
        {
            switch (subType)
            {
#if DIRECTX11_1
                case SharpDX.XInput.DeviceSubType.ArcadePad:
                    Debug.WriteLine("XInput's DeviceSubType.ArcadePad is not supported in XNA");
                    return Input.GamePadType.Unknown; // TODO: Should this be BigButtonPad?

                case SharpDX.XInput.DeviceSubType.FlightStick:
                    return Input.GamePadType.FlightStick;

                case SharpDX.XInput.DeviceSubType.GuitarAlternate:
                    return Input.GamePadType.AlternateGuitar;

                case SharpDX.XInput.DeviceSubType.GuitarBass:
                    // Note: XNA doesn't distinguish between Guitar and GuitarBass, but 
                    // GuitarBass is identical to Guitar in XInput, distinguished only
                    // to help setup for those controllers. 
                    return Input.GamePadType.Guitar;

                case SharpDX.XInput.DeviceSubType.Unknown: return Input.GamePadType.Unknown;
#endif
                case DeviceSubType.ArcadeStick: return GamePadType.ArcadeStick;
                case DeviceSubType.DancePad: return GamePadType.DancePad;
                case DeviceSubType.DrumKit: return GamePadType.DrumKit;
                case DeviceSubType.Gamepad: return GamePadType.GamePad;
                case DeviceSubType.Guitar: return GamePadType.Guitar;
                case DeviceSubType.Wheel: return GamePadType.Wheel;

                default:
                    Debug.WriteLine("Unknown XInput DeviceSubType: {0}", subType.ToString());
                    return GamePadType.Unknown;
            }
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            // If the device was disconneced then wait for 
            // the timeout to elapsed before we test it again.
            if (!_connected[index] && !HasDisconnectedTimeoutElapsed(index))
                return default;

            // Check to see if the device is connected.
            var controller = _controllers[index];
            _connected[index] = controller.IsConnected;

            // If the device is disconnected retry it after the
            // timeout period has elapsed to avoid the overhead.
            if (!_connected[index])
            {
                SetDisconnectedTimeout(index);
                return default;
            }

            var capabilities = controller.GetCapabilities(DeviceQueryType.Any);
            var buttons = capabilities.Gamepad.Buttons;
            bool hasForceFeedback = (capabilities.Flags & CapabilityFlags.FfbSupported) != 0;

            return new GamePadCapabilities(
                // digital buttons
                hasAButton: (buttons & GBF.A) != 0,
                hasBackButton: (buttons & GBF.Back) != 0,
                hasBButton: (buttons & GBF.B) != 0,
                hasBigButton: false, // TODO: what IS this? Is it related to amePadType.BigGamePad?
                hasDPadDownButton: (buttons & GBF.DPadDown) != 0,
                hasDPadLeftButton: (buttons & GBF.DPadLeft) != 0,
                hasDPadRightButton: (buttons & GBF.DPadRight) != 0,
                hasDPadUpButton: (buttons & GBF.DPadUp) != 0,
                hasLeftShoulderButton: (buttons & GBF.LeftShoulder) != 0,
                hasLeftStickButton: (buttons & GBF.LeftThumb) != 0,
                hasRightShoulderButton: (buttons & GBF.RightShoulder) != 0,
                hasRightStickButton: (buttons & GBF.RightThumb) != 0,
                hasStartButton: (buttons & GBF.Start) != 0,
                hasXButton: (buttons & GBF.X) != 0,
                hasYButton: (buttons & GBF.Y) != 0,

                // analog controls
                hasRightTrigger: capabilities.Gamepad.RightTrigger > 0,
                hasRightXThumbStick: capabilities.Gamepad.RightThumbX != 0,
                hasRightYThumbStick: capabilities.Gamepad.RightThumbY != 0,
                hasLeftTrigger: capabilities.Gamepad.LeftTrigger > 0,
                hasLeftXThumbStick: capabilities.Gamepad.LeftThumbX != 0,
                hasLeftYThumbStick: capabilities.Gamepad.LeftThumbY != 0,

                // vibration
#if DIRECTX11_1
                hasLeftVibrationMotor: hasForceFeedback && capabilities.Vibration.LeftMotorSpeed > 0,
                hasRightVibrationMotor: hasForceFeedback && capabilities.Vibration.RightMotorSpeed > 0,
#else
                hasLeftVibrationMotor: capabilities.Vibration.LeftMotorSpeed > 0,
                hasRightVibrationMotor: capabilities.Vibration.RightMotorSpeed > 0,
#endif

                // other
                displayName: null,
                identifier: null,
                isConnected: controller.IsConnected,
                gamePadType: DeviceSubTypeToGamePadType(capabilities.SubType),
                hasVoiceSupport: (capabilities.Flags & CapabilityFlags.VoiceSupported) != 0);
        }

        private static GamePadState GetDefaultState()
        {
            return new GamePadState(
                false, default, default, new GamePadButtons(Back ? Buttons.Back : 0), default);
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            // If the device was disconneced then wait for 
            // the timeout to elapsed before we test it again.
            if (!_connected[index] && !HasDisconnectedTimeoutElapsed(index))
                return GetDefaultState();

            int packetNumber = 0;

            // Try to get the controller state.
            var gamepad = new Gamepad();
            try
            {
                var controller = _controllers[index];
                _connected[index] = controller.GetState(out State xistate);
                packetNumber = xistate.PacketNumber;
                gamepad = xistate.Gamepad;
            }
            catch (Exception)
            {
            }

            // If the device is disconnected retry it after the
            // timeout period has elapsed to avoid the overhead.
            if (!_connected[index])
            {
                SetDisconnectedTimeout(index);
                return GetDefaultState();
            }

            var thumbSticks = new GamePadThumbSticks(
                leftPosition: new Vector2(gamepad.LeftThumbX, gamepad.LeftThumbY) / (float)short.MaxValue,
                rightPosition: new Vector2(gamepad.RightThumbX, gamepad.RightThumbY) / (float)short.MaxValue,
                    leftDeadZoneMode: leftDeadZoneMode,
                    rightDeadZoneMode: rightDeadZoneMode);

            var triggers = new GamePadTriggers(
                    leftTrigger: gamepad.LeftTrigger / (float)byte.MaxValue,
                    rightTrigger: gamepad.RightTrigger / (float)byte.MaxValue);

            var dpadState = new GamePadDPad(
                upValue: ConvertToButtonState(gamepad.Buttons, GBF.DPadUp),
                downValue: ConvertToButtonState(gamepad.Buttons, GBF.DPadDown),
                leftValue: ConvertToButtonState(gamepad.Buttons, GBF.DPadLeft),
                rightValue: ConvertToButtonState(gamepad.Buttons, GBF.DPadRight));

            var buttons = ConvertToButtons(
                buttonFlags: gamepad.Buttons,
                leftTrigger: gamepad.LeftTrigger,
                rightTrigger: gamepad.RightTrigger);

            return new GamePadState(true, thumbSticks, triggers, buttons, dpadState, packetNumber);
        }

        private static ButtonState ConvertToButtonState(
            GBF buttonFlags,
            GBF desiredButton)
        {
            return ((buttonFlags & desiredButton) == desiredButton) ? ButtonState.Pressed : ButtonState.Released;
        }

        private static Buttons AddButtonIfPressed(
            GBF buttonFlags,
            GBF xInputButton,
            Buttons xnaButton)
        {
            var buttonState = ((buttonFlags & xInputButton) == xInputButton) ? ButtonState.Pressed : ButtonState.Released;
            return buttonState == ButtonState.Pressed ? xnaButton : 0;
        }

        private static GamePadButtons ConvertToButtons(GBF buttonFlags,
            byte leftTrigger,
            byte rightTrigger)
        {
            var ret = (Buttons)0;
            ret |= AddButtonIfPressed(buttonFlags, GBF.A, Buttons.A);
            ret |= AddButtonIfPressed(buttonFlags, GBF.B, Buttons.B);
            ret |= AddButtonIfPressed(buttonFlags, GBF.Back, Buttons.Back);
            ret |= AddButtonIfPressed(buttonFlags, GBF.DPadDown, Buttons.DPadDown);
            ret |= AddButtonIfPressed(buttonFlags, GBF.DPadLeft, Buttons.DPadLeft);
            ret |= AddButtonIfPressed(buttonFlags, GBF.DPadRight, Buttons.DPadRight);
            ret |= AddButtonIfPressed(buttonFlags, GBF.DPadUp, Buttons.DPadUp);
            ret |= AddButtonIfPressed(buttonFlags, GBF.LeftShoulder, Buttons.LeftShoulder);
            ret |= AddButtonIfPressed(buttonFlags, GBF.RightShoulder, Buttons.RightShoulder);
            ret |= AddButtonIfPressed(buttonFlags, GBF.LeftThumb, Buttons.LeftStick);
            ret |= AddButtonIfPressed(buttonFlags, GBF.RightThumb, Buttons.RightStick);
            ret |= AddButtonIfPressed(buttonFlags, GBF.Start, Buttons.Start);
            ret |= AddButtonIfPressed(buttonFlags, GBF.X, Buttons.X);
            ret |= AddButtonIfPressed(buttonFlags, GBF.Y, Buttons.Y);

            if (leftTrigger >= Gamepad.TriggerThreshold)
                ret |= Buttons.LeftTrigger;

            if (rightTrigger >= Gamepad.TriggerThreshold)
                ret |= Buttons.RightTrigger;

            // Check for the hardware back button.
            if (Back)
                ret |= Buttons.Back;

            return new GamePadButtons(ret);
        }

        private static bool PlatformSetVibration(
            int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            if (!_connected[index])
            {
                if (!HasDisconnectedTimeoutElapsed(index))
                    return false;

                if (!_controllers[index].IsConnected)
                {
                    SetDisconnectedTimeout(index);
                    return false;
                }
                _connected[index] = true;
            }

            SharpDX.Result result;
            try
            {
                var vibration = new Vibration
                {
                    LeftMotorSpeed = (ushort)(leftMotor * ushort.MaxValue),
                    RightMotorSpeed = (ushort)(rightMotor * ushort.MaxValue),
                };
                result = _controllers[index].SetVibration(vibration);
            }
            catch (SharpDX.SharpDXException ex)
            {
                const int deviceNotConnectedHResult = unchecked((int)0x8007048f);
                if (ex.HResult == deviceNotConnectedHResult)
                {
                    _connected[index] = false;
                    SetDisconnectedTimeout(index);
                    return false;
                }
                throw;
            }
            return result == SharpDX.Result.Ok;
        }

        private static bool HasDisconnectedTimeoutElapsed(int index)
        {
            return _timeout[index] <= DateTime.UtcNow.Ticks;
        }

        private static void SetDisconnectedTimeout(int index)
        {
            _timeout[index] = DateTime.UtcNow.Ticks + TimeoutTicks;
        }
    }
}

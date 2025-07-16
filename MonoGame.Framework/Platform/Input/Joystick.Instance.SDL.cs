// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Input
{
    public static partial class Joystick
    {
        internal class Instance
        {
            private string _identifier;

            public int Id { get; }
            public IntPtr Device { get; private set; }
            public Guid Guid { get; private set; }

            public int[] Axes { get; private set; }
            public ButtonState[] Buttons { get; private set; }
            public JoystickHat[] Hats { get; private set; }

            public Instance(int id, IntPtr device)
            {
                Id = id;
                Device = device;

                Axes = Array.Empty<int>();
                Buttons = Array.Empty<ButtonState>();
                Hats = Array.Empty<JoystickHat>();
            }

            public JoystickCapabilities GetCapabilities(int index)
            {
                var guid = SDL.Joystick.GetGUID(Device);
                if (Guid != guid)
                {
                    Guid = guid;
                    _identifier = Guid.ToString();
                }

                return new JoystickCapabilities(
                    isConnected: true,
                    displayName: SDL.Joystick.GetName(Device),
                    identifier: _identifier,
                    isGamepad: SDL.GameController.IsGameController(index) == 1,
                    axisCount: SDL.Joystick.NumAxes(Device),
                    buttonCount: SDL.Joystick.NumButtons(Device),
                    hatCount: SDL.Joystick.NumHats(Device));
            }

            public int[] GetAxes(in JoystickCapabilities jcap)
            {
                if (Axes.Length != jcap.AxisCount)
                    Axes = new int[jcap.AxisCount];

                for (var i = 0; i < Axes.Length; i++)
                    Axes[i] = SDL.Joystick.GetAxis(Device, i);
                return Axes;
            }

            public ButtonState[] GetButtons(in JoystickCapabilities jcap)
            {
                if (Buttons.Length != jcap.ButtonCount)
                    Buttons = new ButtonState[jcap.ButtonCount];

                for (var i = 0; i < Buttons.Length; i++)
                    Buttons[i] = (SDL.Joystick.GetButton(Device, i) == 0) ? ButtonState.Released : ButtonState.Pressed;
                return Buttons;
            }

            public JoystickHat[] GetHats(in JoystickCapabilities jcap)
            {
                if (Hats.Length != jcap.HatCount)
                    Hats = new JoystickHat[jcap.HatCount];

                for (var i = 0; i < Hats.Length; i++)
                {
                    var hatstate = SDL.Joystick.GetHat(Device, i);

                    Hats[i] = new JoystickHat(
                        up: (hatstate & SDL.Joystick.Hat.Up) != 0 ? ButtonState.Pressed : ButtonState.Released,
                        down: (hatstate & SDL.Joystick.Hat.Down) != 0 ? ButtonState.Pressed : ButtonState.Released,
                        left: (hatstate & SDL.Joystick.Hat.Left) != 0 ? ButtonState.Pressed : ButtonState.Released,
                        right: (hatstate & SDL.Joystick.Hat.Right) != 0 ? ButtonState.Pressed : ButtonState.Released);
                }
                return Hats;
            }

            public void Close()
            {
                SDL.Joystick.Close(Device);
                Device = IntPtr.Zero;
            }
        }
    }
}

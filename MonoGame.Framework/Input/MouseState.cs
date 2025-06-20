// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;

namespace MonoGame.Framework.Input
{
    /// <summary>
    /// Represents a mouse state with cursor position and button press information.
    /// </summary>
    public struct MouseState : IEquatable<MouseState>
    {
        #region Properties

        /// <summary>
        /// Gets the state of all the mouse buttons.
        /// </summary>
        public MouseButton Buttons { get; internal set; }

        /// <summary>
        /// Gets horizontal position of the cursor in relation to the window.
        /// </summary>
        public int X { get; internal set; }

        /// <summary>
        /// Gets vertical position of the cursor in relation to the window.
        /// </summary>
        public int Y { get; internal set; }

        /// <summary>
        /// Gets the cumulative horizontal scroll wheel value since the game start.
        /// </summary>
        public int HorizontalScroll { get; internal set; }

        /// <summary>
        /// Gets the cumulative vertical scroll wheel value since the game start.
        /// </summary>
        public int VerticalScroll { get; internal set; }

        /// <summary>
        /// Gets the cursor position in relation to the window.
        /// </summary>
        public readonly Point Position => new Point(X, Y);

        /// <summary>
        /// Gets the cumulative scroll wheel values since the game start.
        /// </summary>
        public readonly Vector2 Scroll => new Vector2(HorizontalScroll, VerticalScroll);

        /// <summary>
        /// Gets state of the left mouse button.
        /// </summary>
        public ButtonState LeftButton
        {
            get => Buttons.HasFlags(MouseButton.Left) ? ButtonState.Pressed : ButtonState.Released;
            internal set => Buttons = value.ToBool() ? Buttons | MouseButton.Left : Buttons & ~MouseButton.Left;
        }

        /// <summary>
        /// Gets state of the middle mouse button.
        /// </summary>
        public ButtonState MiddleButton
        {
            get => Buttons.HasFlags(MouseButton.Middle) ? ButtonState.Pressed : ButtonState.Released;
            internal set => Buttons = value.ToBool() ? Buttons | MouseButton.Middle : Buttons & ~MouseButton.Middle;
        }

        /// <summary>
        /// Gets state of the right mouse button.
        /// </summary>
        public ButtonState RightButton
        {
            get => Buttons.HasFlags(MouseButton.Right) ? ButtonState.Pressed : ButtonState.Released;
            internal set => Buttons = value.ToBool() ? Buttons | MouseButton.Right : Buttons & ~MouseButton.Right;
        }

        /// <summary>
        /// Gets state of the XButton1.
        /// </summary>
        public ButtonState XButton1
        {
            get => Buttons.HasFlags(MouseButton.X1) ? ButtonState.Pressed : ButtonState.Released;
            internal set => Buttons = value.ToBool() ? Buttons | MouseButton.X1 : Buttons & ~MouseButton.X1;
        }

        /// <summary>
        /// Gets state of the XButton2.
        /// </summary>
        public ButtonState XButton2
        {
            get => Buttons.HasFlags(MouseButton.X2) ? ButtonState.Pressed : ButtonState.Released;
            internal set => Buttons = value.ToBool() ? Buttons | MouseButton.X2 : Buttons & ~MouseButton.X2;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MouseState.
        /// </summary>
        /// <param name="x">Horizontal position of the mouse in relation to the window.</param>
        /// <param name="y">Vertical position of the mouse in relation to the window.</param>
        /// <param name="scroll">Mouse scroll value.</param>
        /// <param name="horizontalScroll">Mouse horizontal scroll value.</param>
        /// <param name="buttons">Mouse button state.</param>
        /// <remarks>
        /// Normally <see cref="Mouse.GetState()"/> should be used to get mouse current state.
        /// The constructor is provided for simulating mouse input.
        /// </remarks>
        public MouseState(int x, int y, int scroll, int horizontalScroll, MouseButton buttons)
        {
            X = x;
            Y = y;
            VerticalScroll = scroll;
            HorizontalScroll = horizontalScroll;
            Buttons = buttons;
        }

        /// <summary>
        /// Initializes a new instance of the MouseState.
        /// </summary>
        /// <param name="x">Horizontal position of the mouse in relation to the window.</param>
        /// <param name="y">Vertical position of the mouse in relation to the window.</param>
        /// <param name="scroll">Mouse scroll value.</param>
        /// <param name="horizontalScroll">Mouse horizontal scroll value.</param>
        /// <param name="leftButton">Left mouse button's state.</param>
        /// <param name="middleButton">Middle mouse button's state.</param>
        /// <param name="rightButton">Right mouse button's state.</param>
        /// <param name="xButton1">XButton1's state.</param>
        /// <param name="xButton2">XButton2's state.</param>
        /// <remarks>
        /// Normally <see cref="Mouse.GetState()"/> should be used to get mouse current state.
        /// The constructor is provided for simulating mouse input.
        /// </remarks>
        public MouseState(
            int x,
            int y,
            int scroll,
            int horizontalScroll,
            ButtonState leftButton,
            ButtonState middleButton,
            ButtonState rightButton,
            ButtonState xButton1,
            ButtonState xButton2) : this(
                x, y, scroll, horizontalScroll,
                (leftButton == ButtonState.Pressed ? MouseButton.Left : 0) |
                (rightButton == ButtonState.Pressed ? MouseButton.Right : 0) |
                (middleButton == ButtonState.Pressed ? MouseButton.Middle : 0) |
                (xButton1 == ButtonState.Pressed ? MouseButton.X1 : 0) |
                (xButton2 == ButtonState.Pressed ? MouseButton.X2 : 0))
        {
        }

        #endregion

        #region Equals

        public readonly bool Equals(MouseState other) => this == other;

        public override readonly bool Equals(object? obj) => obj is MouseState other && this == other;

        /// <summary>
        /// Compares whether two MouseState instances are equal.
        /// </summary>
        /// <param name="left">MouseState instance on the left of the equal sign.</param>
        /// <param name="right">MouseState instance  on the right of the equal sign.</param>
        /// <returns>true if the instances are equal; false otherwise.</returns>
        public static bool operator ==(in MouseState left, in MouseState right)
        {
            return left.X == right.X
                && left.Y == right.Y
                && left.Buttons == right.Buttons
                && left.VerticalScroll == right.VerticalScroll
                && left.HorizontalScroll == right.HorizontalScroll;
        }

        /// <summary>
        /// Compares whether two <see cref="MouseState"/> instances are not equal.
        /// </summary>
        /// <param name="left">Instance on the left of the equal sign.</param>
        /// <param name="right">Instance on the right of the equal sign.</param>
        /// <returns>true if the objects are not equal; false otherwise.</returns>
        public static bool operator !=(in MouseState left, in MouseState right)
        {
            return !(left == right);
        }

        #endregion

        #region Object overrides

        /// <summary>
        /// Gets the hash code for <see cref="MouseState"/> instance.
        /// </summary>
        /// <returns>Hash code of the object.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Buttons, X, Y, HorizontalScroll, VerticalScroll);
        }

        /// <summary>
        /// Returns a string describing the mouse state.
        /// </summary>
        public override readonly string ToString()
        {
            return 
                "{Buttons=" + Buttons +
                ", X=" + X +
                ", Y=" + Y +
                ", HorizontalScroll=" + HorizontalScroll +
                ", VerticalScroll=" + VerticalScroll +
                "}";
        }

        #endregion
    }
}

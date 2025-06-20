// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Numerics;

namespace MonoGame.Framework.Input.Touch
{
    public struct TouchLocation : IEquatable<TouchLocation>
    {
        /// <summary>
        /// Helper for assigning an invalid touch location.
        /// </summary>
        internal static TouchLocation Invalid => default;

        private Vector2 _position;
        private Vector2 _previousPosition;
        private TouchLocationState _previousState;
        private float _previousPressure;

        /// <summary>
        /// True if this touch was pressed and released on the same frame.
        /// In this case we will keep it around for the user to get by GetState that frame.
        /// However if they do not call GetState that frame, this touch will be forgotten.
        /// </summary>
        internal bool SameFrameReleased;

        #region Properties

        internal TimeSpan PressTimestamp { get; private set; }
        internal Vector2 PressPosition { get; private set; }
        internal Vector2 Velocity { get; private set; }
        internal TimeSpan Timestamp { get; private set; }

        public int Id { get; private set; }
        public Vector2 Position => _position;
        public float Pressure { get; private set; }
        public TouchLocationState State { get; private set; }

        #endregion

        #region Constructors

        public TouchLocation(int id, TouchLocationState state, Vector2 position)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero)
        {
        }

        public TouchLocation(   int id, TouchLocationState state, Vector2 position, 
                                TouchLocationState previousState, Vector2 previousPosition)
            : this(id, state, position, previousState, previousPosition, TimeSpan.Zero)
        {
        }

        internal TouchLocation(int id, TouchLocationState state, Vector2 position, TimeSpan timestamp)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero, timestamp)
        {
        }

        internal TouchLocation(int id, TouchLocationState state, Vector2 position,
            TouchLocationState previousState, Vector2 previousPosition, TimeSpan timestamp)
        {
            Id = id;
            State = state;
            _position = position;
            Pressure = 0f;

            _previousState = previousState;
            _previousPosition = previousPosition;
            _previousPressure = 0f;

            Timestamp = timestamp;
            Velocity = Vector2.Zero;

            // If this is a pressed location then store the 
            // current position and timestamp as pressed.
            if (state == TouchLocationState.Pressed)
            {
                PressPosition = _position;
                PressTimestamp = Timestamp;
            }
            else
            {
                PressPosition = Vector2.Zero;
                PressTimestamp = TimeSpan.Zero;
            }

            SameFrameReleased = false;
        }

        #endregion

        /// <summary>
        /// Returns a copy of the touch with the state changed to moved.
        /// </summary>
        /// <returns>The new touch location.</returns>
        internal TouchLocation AsMovedState()
        {
            var touch = this;

            // Store the current state as the previous.
            touch._previousState = touch.State;
            touch._previousPosition = touch._position;
            touch._previousPressure = touch.Pressure;

            // Set the new state.
            touch.State = TouchLocationState.Moved;
            
            return touch;
        }

        /// <summary>
        /// Updates the touch location using the new event.
        /// </summary>
        /// <param name="touchEvent">The next event for this touch location.</param>
        internal bool UpdateState(TouchLocation touchEvent)
        {
            Debug.Assert(Id == touchEvent.Id, "The touch event must have the same Id!");
            Debug.Assert(State != TouchLocationState.Released, "We shouldn't be changing state on a released location!");
            Debug.Assert(touchEvent.State == TouchLocationState.Moved || touchEvent.State == TouchLocationState.Released, 
                "The new touch event should be a move or a release!");
            Debug.Assert(touchEvent.Timestamp >= Timestamp, "The touch event is older than our timestamp!");

            // Store the current state as the previous one.
            _previousPosition = _position;
            _previousState = State;
            _previousPressure = Pressure;

            // Set the new state.
            _position = touchEvent._position;
            if (touchEvent.State == TouchLocationState.Released)
                State = touchEvent.State;
            Pressure = touchEvent.Pressure;

            // If time has elapsed then update the velocity.
            var delta = _position - _previousPosition;
            var elapsed = touchEvent.Timestamp - Timestamp;
            if (elapsed > TimeSpan.Zero)
            {
                // Use a simple low pass filter to accumulate velocity.
                var velocity = delta / (float)elapsed.TotalSeconds;
                Velocity += (velocity - Velocity) * 0.45f;
            }

            //Going straight from pressed to released on the same frame
            if (_previousState == TouchLocationState.Pressed && State == TouchLocationState.Released && elapsed == TimeSpan.Zero)
            {
                //Lie that we are pressed for now
                SameFrameReleased = true;
                State = TouchLocationState.Pressed;
            }

            // Set the new timestamp.
            Timestamp = touchEvent.Timestamp;

            // Return true if the state actually changed.
            return State != _previousState || delta.LengthSquared() > 0.001f;
        }

        #region Equals

        public static bool operator ==(in TouchLocation a, in TouchLocation b)
        {
            return a.Id == b.Id &&
                   a.State == b.State &&
                   a._position == b._position &&
                   a._previousState == b._previousState &&
                   a._previousPosition == b._previousPosition;
        }

        public static bool operator !=(in TouchLocation a, in TouchLocation b) => !(a == b);

        public bool Equals(TouchLocation other) => this == other;
        public override bool Equals(object? obj) => obj is TouchLocation other && Equals(other);

        #endregion

        public override int GetHashCode() => Id;

        public override string ToString()
        {
            return
                "Touch id:" + Id +
                " state:" + State +
                " position:" + _position +
                " pressure:" + Pressure +
                " prevState:" + _previousState +
                " prevPosition:" + _previousPosition +
                " previousPressure:" + _previousPressure;
        }

        public bool TryGetPreviousLocation(out TouchLocation previousLocation)
        {
            if (_previousState == TouchLocationState.Invalid)
            {
                previousLocation = default;
                return false;
            }

            previousLocation = new TouchLocation
            {
                Id = Id,
                State = _previousState,
                _position = _previousPosition,
                _previousState = TouchLocationState.Invalid,
                _previousPosition = Vector2.Zero,
                Pressure = _previousPressure,
                _previousPressure = 0f,
                Timestamp = Timestamp,
                PressPosition = PressPosition,
                PressTimestamp = PressTimestamp,
                Velocity = Velocity,
                SameFrameReleased = SameFrameReleased
            };
            return true;
        }

        internal void AgeState()
        {
            if (State == TouchLocationState.Moved)
            {
                _previousState = State;
                _previousPosition = _position;
                _previousPressure = Pressure;
            }
            else
            {
                Debug.Assert(State == TouchLocationState.Pressed,
                    "Can only age the state of touches that are in the Pressed State");

                _previousState = State;
                _previousPosition = _position;
                _previousPressure = Pressure;

                if (SameFrameReleased)
                    State = TouchLocationState.Released;
                else
                    State = TouchLocationState.Moved;
            }
        }
    }
}

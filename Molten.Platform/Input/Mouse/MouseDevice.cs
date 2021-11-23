﻿using Molten.Graphics;
using System.Collections.Generic;

namespace Molten.Input
{
    public delegate void MouseEventHandler(MouseDevice mouse, MouseButtonState state);

    /// <summary>
    /// Represents an implementation of a mouse or pointer device.
    /// </summary>
    public abstract class MouseDevice : InputDevice<MouseButtonState, MouseButton>
    {
        /// <summary>
        /// Occurs when the mouse cursor was inside the parent window/control, but just left it.
        /// </summary>
        public event MouseEventHandler OnLeaveSurface;

        /// <summary>
        /// Occurs when the mouse cursor was outside of the parent window/control, but just entered it.
        /// </summary>
        public event MouseEventHandler OnEnterSurface;

        public event MouseEventHandler OnMoved;

        public event MouseEventHandler OnHeld;

        public event MouseEventHandler OnHover;

        public event MouseEventHandler OnButtonDown;

        public event MouseEventHandler OnButtonUp;

        /// <summary>
        /// Invoked when the mouse performs a vertical scroll action.
        /// </summary>
        public event MouseEventHandler OnVScroll;

        /// <summary>
        /// Invoked when the mouse performs a horizontal scroll action.
        /// </summary>
        public event MouseEventHandler OnHScroll;

        INativeSurface _surface;
        Vector2I _relativePos;
        Vector2I _absolutePos;
        bool _cursorVisible;
        bool _wasInsideControl;

        public MouseDevice(InputManager manager) : 
            base(manager, manager.Settings.MouseBufferSize)
        {

        }

        /// <summary>Positions the mouse cursor at the center of the currently-bound <see cref="IInputCamera.OutputSurface"/>.</summary>
        public void Center()
        {
            if (_surface == null)
                return;

            Rectangle winBounds = _surface.Bounds;
            AbsolutePosition = winBounds.Center;
        }

        private Vector2I ToRelativePosition(Vector2I pos)
        {
            Rectangle oBounds = _surface.Bounds;
            return pos - new Vector2I(oBounds.X, oBounds.Y);
        }

        private Vector2I ToAbsolutePosition(Vector2I pos)
        {
            Rectangle oBounds = _surface.Bounds;
            return pos + new Vector2I(oBounds.X, oBounds.Y);
        }

        protected override int GetStateID(ref MouseButtonState state)
        {
            return (int)state.Button;
        }

        protected override int TranslateStateID(MouseButton idValue)
        {
            return (int)idValue;
        }

        protected override void ProcessState(ref MouseButtonState newState, ref MouseButtonState prevState)
        {
            Delta = Vector2I.Zero;

            // Is the cursor constrained to it's parent control/window?
            Rectangle sBounds = _surface.Bounds;
            if (IsConstrained)
            {
                if (newState.Position.X < sBounds.X)
                    newState.Position.X = sBounds.X;
                if (newState.Position.Y < sBounds.Y)
                    newState.Position.Y = sBounds.Y;
                if (newState.Position.X > sBounds.Width)
                    newState.Position.X = sBounds.Width;
                if (newState.Position.Y > sBounds.Height)
                    newState.Position.Y = sBounds.Height;
            }

            // Check if the cursor has gone outside of the bound control/window 
            bool insideControl = newState.Position.X >= sBounds.Left &&
                newState.Position.Y >= sBounds.Top &&
                newState.Position.X <= sBounds.Right &&
                newState.Position.Y <= sBounds.Bottom;            

            // Calculate delta
            if (newState.Action != InputAction.None && 
                prevState.Action != InputAction.None && prevState.Action != InputAction.Released)
            {
                newState.Delta = newState.Position - prevState.Position;
                newState.PressTimestamp = prevState.PressTimestamp;
                Delta = newState.Delta;
            }

            // Perform some error checking input action
            if (newState.Action == InputAction.Held ||
                (newState.Action == InputAction.Pressed &&
                prevState.Action == InputAction.Pressed))
            {
                newState.Action = newState.Delta != Vector2I.Zero ? 
                    InputAction.Moved : InputAction.Held;
            }

            AbsolutePosition = newState.Position;

            CheckInside(insideControl, ref newState);

            switch (newState.Action)
            {
                case InputAction.Held:
                    OnHeld?.Invoke(this, newState);
                    break;

                case InputAction.Moved:
                    OnMoved?.Invoke(this, newState);
                    break;

                case InputAction.Pressed:
                    OnButtonDown?.Invoke(this, newState);
                    break;

                case InputAction.Released:
                    OnButtonUp?.Invoke(this, newState);
                    break;

                case InputAction.VerticalScroll:
                    ScrollWheel.SetValues(newState.Delta.Y);
                    OnVScroll?.Invoke(this, newState);
                    break;

                case InputAction.HorizontalScroll:
                    ScrollWheel.SetValues(newState.Delta.X);
                    OnHScroll?.Invoke(this, newState);
                    break;

                case InputAction.Hover:
                    OnHover?.Invoke(this, newState);
                    break;
            }
        }

        private void CheckInside(bool insideControl, ref MouseButtonState state)
        {
            if (insideControl && !_wasInsideControl)
                OnEnterSurface?.Invoke(this, state);
            else if (!insideControl && _wasInsideControl)
                OnLeaveSurface?.Invoke(this, state);

            _wasInsideControl = insideControl;
        }

        protected override bool GetIsDown(ref MouseButtonState state)
        {
            if (state.Button != MouseButton.None)
            {
                return state.Action == InputAction.Pressed ||
                    state.Action == InputAction.Held ||
                    state.Action == InputAction.Moved;
            }

            return false;
        }

        protected override bool GetIsHeld(ref MouseButtonState state)
        {
            return state.Action == InputAction.Held || state.Action == InputAction.Moved;
        }

        protected override bool GetIsTapped(ref MouseButtonState state)
        {
            return state.Action == InputAction.Pressed;
        }

        protected override void OnUpdate(Timing time) { }

        protected abstract void OnSetCursorPosition(Vector2I absolute, Vector2I relative);

        /// <summary>
        /// Invoked when cursor visibility has changed.
        /// </summary>
        /// <param name="visible"></param>
        protected abstract void OnSetCursorVisibility(bool visible);

        /// <summary>Returns the amount the mouse cursor has moved a long X and Y since the last frame/update.</summary>
        public Vector2I Delta { get; private set; }

        /// <summary>
        /// Gets the vertical scroll wheel, if one is present. Returns null if not.
        /// </summary>
        public abstract InputScrollWheel ScrollWheel { get; protected set; }

        /// <summary>
        /// Gets the horizontal scroll wheel, if one is present. Returns null if not.
        /// </summary>
        public abstract InputScrollWheel HScrollWheel { get; protected set; }

        /// <summary>
        /// Gets the position of the mouse cursor, relative to the bound <see cref="INativeSurface"/>.
        /// </summary>
        public Vector2I Position
        {
            get => _relativePos;
            set
            {
                _relativePos = value;
                _absolutePos = ToAbsolutePosition(_relativePos);
                OnSetCursorPosition(_absolutePos, _relativePos);
            }
        }

        /// <summary>
        /// Gets or sets the absolute position of the mouse cursor. 
        /// This is the absolute native position across all of the client's display area/screens.
        /// </summary>
        public Vector2I AbsolutePosition
        {
            get => _absolutePos;
            set
            {
                _absolutePos = value;
                _relativePos = ToRelativePosition(_absolutePos);
                OnSetCursorPosition(_absolutePos, _relativePos);
            }
        }

        /// <summary>Gets or sets whether or not the native mouse cursor is visible.</summary>
        public bool IsCursorVisible
        {
            get => _cursorVisible;
            set
            {
                if(_cursorVisible != value)
                {
                    _cursorVisible = value;
                    OnSetCursorVisibility(value);
                }
            }
        }

        /// <summary>Gets or sets whether or not the mouse is contrained to the bounds of the main output.</summary>
        public bool IsConstrained { get; set; }

        /// <summary>
        /// Gets whether or not the cursor inside the bounds of the bound <see cref="INativeSurface"/>.
        /// </summary>
        public bool IsCursorInSurface { get; private set; }
    }
}
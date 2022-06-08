using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.Input
{
    public delegate void PointingDeviceHandler<T>(PointingDevice<T> mouse, PointerState<T> state) where T : struct;

    public abstract class PointingDevice<T> : InputDevice<PointerState<T>, T>
        where T : struct
    {
        /// <summary>
        /// Occurs when the mouse cursor was inside the parent window/control, but just left it.
        /// </summary>
        public event PointingDeviceHandler<T> OnLeaveSurface;

        /// <summary>
        /// Occurs when the mouse cursor was outside of the parent window/control, but just entered it.
        /// </summary>
        public event PointingDeviceHandler<T> OnEnterSurface;

        public event PointingDeviceHandler<T> OnMoved;

        public event PointingDeviceHandler<T> OnHeld;

        public event PointingDeviceHandler<T> OnHover;

        public event PointingDeviceHandler<T> OnButtonDown;

        public event PointingDeviceHandler<T> OnButtonUp;

        INativeSurface _surface;
        bool _wasInsideControl;

        protected override List<InputDeviceFeature> OnInitialize(InputService service)
        {
            InitializeBuffer(service.Settings.Input.PointerBufferSize);
            return null;
        }

        /// <summary>Positions the mouse cursor at the center of the currently-bound <see cref="IInputCamera.OutputSurface"/>.</summary>
        public void Center()
        {
            if (_surface == null)
                return;

            Rectangle winBounds = _surface.Bounds;
            Position = (Vector2F)winBounds.Center;
            OnSetPointerPosition(Position);
        }

        protected override void OnBind(INativeSurface surface)
        {
            _surface = surface;
        }

        protected override void OnUnbind(INativeSurface surface)
        {
            // Only un-set if the surface passed in matches the bound one, otherwise ignore.
            if (_surface == surface)
                _surface = null;
        }

        protected override bool ProcessState(ref PointerState<T> newState, ref PointerState<T> prevState)
        {
            if (_surface == null)
                return true;

            Delta = Vector2F.Zero;

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

            // Prioritise press/release actions over anything else
            if (newState.UpdateID == prevState.UpdateID)
            {
                if (newState.Action == InputAction.Held ||
                    newState.Action == InputAction.Moved ||
                    newState.Action == InputAction.Hover)
                {
                    if (prevState.Action == InputAction.Pressed || prevState.Action == InputAction.Released)
                        newState = prevState;
                }
            }

            // Calculate delta
            if (newState.Action != InputAction.None &&
                prevState.Action != InputAction.None && prevState.Action != InputAction.Released)
            {
                newState.Delta = newState.Position - prevState.Position;
                newState.PressTimestamp = prevState.PressTimestamp;
                Delta = newState.Delta;
            }

            // Perform error checking on delta vs action
            if (newState.Action == InputAction.Held || newState.Action == InputAction.Moved)
            {
                newState.Action = newState.Delta != Vector2F.Zero ?
                    InputAction.Moved : InputAction.Held;
            }

            Position = newState.Position;

            CheckInside(insideControl, ref newState);

            if (newState.UpdateID == prevState.UpdateID && newState.Action == prevState.Action)
                return true;

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

                case InputAction.Hover:
                    OnHover?.Invoke(this, newState);
                    break;
            }

            return true;
        }

        private void CheckInside(bool insideControl, ref PointerState<T> state)
        {
            if (insideControl && !_wasInsideControl)
                OnEnterSurface?.Invoke(this, state);
            else if (!insideControl && _wasInsideControl)
                OnLeaveSurface?.Invoke(this, state);

            _wasInsideControl = insideControl;
        }

        protected override bool GetIsHeld(ref PointerState<T> state)
        {
            return state.Action == InputAction.Held || state.Action == InputAction.Moved;
        }

        protected override bool GetIsTapped(ref PointerState<T> state)
        {
            return state.Action == InputAction.Pressed && state.UpdateID == Service.UpdateID;
        }

        protected abstract void OnSetPointerPosition(Vector2F position);

        /// <summary>Returns the amount the mouse cursor has moved a long X and Y since the last frame/update.</summary>
        public Vector2F Delta { get; private set; }

        /// <summary>
        /// Gets the position of the mouse cursor, relative to the bound <see cref="INativeSurface"/>.
        /// </summary>
        public Vector2F Position { get; private set; }

        /// <summary>Gets or sets whether or not the mouse is contrained to the bounds of the main output.</summary>
        public bool IsConstrained { get; set; }

        /// <summary>
        /// Gets whether or not the cursor inside the bounds of the bound <see cref="INativeSurface"/>.
        /// </summary>
        public bool IsInSurface { get; private set; }
    }
}

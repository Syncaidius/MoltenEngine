using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.Input
{
    public delegate void PointingDeviceHandler(PointingDevice mouse, PointerState state);

    public abstract class PointingDevice: InputDevice<PointerState, PointerButton>
    {
        /// <summary>
        /// Invoked when any type of input event occurs for the current <see cref="PointingDevice{T}"/>.
        /// </summary>
        public event PointingDeviceHandler OnEvent;
        /// <summary>
        /// Occurs when the mouse cursor was inside the parent window/control, but just left it.
        /// </summary>
        public event PointingDeviceHandler OnLeaveSurface;

        /// <summary>
        /// Occurs when the mouse cursor was outside of the parent window/control, but just entered it.
        /// </summary>
        public event PointingDeviceHandler OnEnterSurface;

        public event PointingDeviceHandler OnMoved;

        public event PointingDeviceHandler OnHeld;

        public event PointingDeviceHandler OnHover;

        public event PointingDeviceHandler OnPressed;

        public event PointingDeviceHandler OnReleased;

        INativeSurface _surface;
        bool _wasInsideControl;
        SettingValue<float> _sensitivitySetting;

        protected override List<InputDeviceFeature> OnInitialize(InputService service)
        {
            _sensitivitySetting = service.Settings.Input.PointerSensitivity;
            Sensitivity = _sensitivitySetting;
            _sensitivitySetting.OnChanged += PointerSensitivity_OnChanged;
            return base.OnInitialize(service);
        }

        private void PointerSensitivity_OnChanged(float oldValue, float newValue)
        {
            Sensitivity = newValue;
        }

        protected override sealed SettingValue<int> GetBufferSizeSetting(InputSettings settings)
        {
            return settings.PointerBufferSize;
        }

        /// <summary>Positions the mouse cursor at the center of the currently-bound <see cref="IInputCamera.Surface"/>.</summary>
        public void Center()
        {
            if (_surface == null)
                return;

            Rectangle winBounds = _surface.RenderBounds;
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

        protected override sealed int GetStateID(ref PointerState state)
        {
            return (int)state.Button;
        }

        protected override sealed int TranslateStateID(PointerButton idValue)
        {
            return (int)idValue;
        }

        protected override void ProcessIdleState()
        {
            base.ProcessIdleState();
            Delta = Vector2F.Zero;
        }

        protected override bool ProcessState(ref PointerState newState, ref PointerState prevState)
        {
            Delta = Vector2F.Zero;

            bool insideControl = false;

            if (_surface != null)
            {
                // Is the cursor constrained to it's parent control/window?
                Rectangle sBounds = _surface.RenderBounds;
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
                insideControl = newState.Position.X >= sBounds.Left &&
                    newState.Position.Y >= sBounds.Top &&
                    newState.Position.X <= sBounds.Right &&
                    newState.Position.Y <= sBounds.Bottom;
            }

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
                newState.Delta = (newState.Position - prevState.Position) * Sensitivity;
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

            OnEvent?.Invoke(this, newState);

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
                    OnPressed?.Invoke(this, newState);
                    break;

                case InputAction.Released:
                    OnReleased?.Invoke(this, newState);
                    break;

                case InputAction.Hover:
                    OnHover?.Invoke(this, newState);
                    break;
            }

            return true;
        }

        private void CheckInside(bool insideControl, ref PointerState state)
        {
            if (insideControl && !_wasInsideControl)
                OnEnterSurface?.Invoke(this, state);
            else if (!insideControl && _wasInsideControl)
                OnLeaveSurface?.Invoke(this, state);

            _wasInsideControl = insideControl;
        }

        protected override bool GetIsHeld(ref PointerState state)
        {
            return state.Action == InputAction.Held || state.Action == InputAction.Moved;
        }

        protected override bool GetIsTapped(ref PointerState state)
        {
            return state.Action == InputAction.Pressed && state.UpdateID == Service.UpdateID;
        }

        protected override bool GetIsDown(ref PointerState state)
        {
            if (state.Button != PointerButton.None)
            {
                return state.Action == InputAction.Pressed ||
                    state.Action == InputAction.Held ||
                    state.Action == InputAction.Moved;
            }

            return false;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _sensitivitySetting.OnChanged -= PointerSensitivity_OnChanged;
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

        /// <summary>
        /// Gets the current level of sensitivity affecting the current <see cref="PointingDevice"/>.
        /// </summary>
        public float Sensitivity { get; private set; } = 1.0f;

        /// <summary>
        /// Gets the type of the current <see cref="PointingDevice"/>.
        /// </summary>
        public abstract PointingDeviceType PointerType { get; }
    }
}

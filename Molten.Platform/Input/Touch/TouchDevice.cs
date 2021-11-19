using Molten.Utility;

namespace Molten.Input
{
    public delegate void TouchGestureHandler<T>(TouchDevice device, T gesture) where T : ITouchGesture;

    public abstract class TouchDevice : InputDevice<TouchPointState, int>
    {
        public TouchDevice(IInputManager manager, Logger log) : 
            base(manager, manager.Settings.TouchBufferSize, log)
        {
            
        }

        public event TouchGestureHandler<Touch2PointGesture> OnPinchGesture;

        /// <summary>
        /// Triggered when a touch point is updated, regardless of type.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> OnTouch;

        /// <summary>
        /// Triggered when an active touch point is moved on the current <see cref="ITouchGesture"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> OnMove;

        /// <summary>
        /// Triggered when a new touch point is pressed down on the current <see cref="ITouchGesture"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> TouchDown;

        /// <summary>
        /// Triggered when an active touch point is released on the current <see cref="ITouchGesture"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> TouchUp;

        /// <summary>
        /// Triggered when an active touch point is held for a period of time on the current <see cref="ITouchGesture"/>.
        /// </summary>
        public event MoltenEventHandler<TouchPointState> TouchHeld;

        /// <summary>
        /// The number of active touch points on the current <see cref="ITouchDevice"/>.
        /// </summary>
        public abstract int TouchPointCount { get; protected set; }

        protected override sealed int TranslateStateID(int idValue) => idValue;

        protected override sealed int GetStateID(ref TouchPointState state) => state.ID;

        protected override void ProcessState(ref TouchPointState newsState, ref TouchPointState prevState)
        {
            // Calculate delta from last pointer state.
            if (newsState.State == InputPressState.Moved && prevState.State != InputPressState.Released)
            {
                newsState.Delta = newsState.Position - newsState.Position;
                OnTouch?.Invoke(newsState);
                OnMove?.Invoke(newsState);
            }
            else
            {
                newsState.Delta = Vector2F.Zero;
                OnTouch?.Invoke(newsState);

                switch (newsState.State)
                {
                    case InputPressState.Pressed: TouchDown?.Invoke(newsState); break;
                    case InputPressState.Released: TouchUp?.Invoke(newsState); break;
                    case InputPressState.Held: TouchHeld?.Invoke(newsState); break;
                }
            }
        }

        protected override bool GetIsHeld(ref TouchPointState state)
        {
            return state.State == InputPressState.Held || state.State == InputPressState.Moved;
        }

        protected override bool GetIsDown(ref TouchPointState state)
        {
            return state.State == InputPressState.Pressed || 
                state.State == InputPressState.Held || 
                state.State == InputPressState.Moved;
        }

        protected override bool GetIsTapped(ref TouchPointState state)
        {
            return state.State == InputPressState.Pressed;
        }
    }
}

namespace Molten.Input
{
    public delegate void KeyHandler(KeyboardDevice device, KeyboardKeyState state);
    public delegate void KeyPressHandler(KeyboardKeyState state);

    public abstract class KeyboardDevice : InputDevice<KeyboardKeyState, KeyCode>
    {
        public KeyboardDevice(IInputManager manager, Logger log) : 
            base(manager, manager.Settings.KeyboardBufferSize, log)
        {

        }

        protected override int TranslateStateID(KeyCode idValue)
        {
            return (int)idValue;
        }

        protected override int GetStateID(ref KeyboardKeyState state)
        {
            return (int)state.Key;
        }

        protected override void ProcessState(ref KeyboardKeyState newState, ref KeyboardKeyState prevState)
        {
            if (newState.State == InputPressState.Held || 
                (newState.State == InputPressState.Pressed && prevState.State == InputPressState.Pressed))
            {
                newState.State = InputPressState.Held;
                newState.PressTimestamp = prevState.PressTimestamp;
                OnKeyHeld?.Invoke(this, newState);
            }else if(newState.State == InputPressState.Pressed)
            {
                OnKeyDown?.Invoke(this, newState);
            }else if(newState.State == InputPressState.Released)
            {
                if (prevState.State != InputPressState.Released && prevState.State != InputPressState.None)
                    newState.PressTimestamp = prevState.PressTimestamp;

                OnKeyUp?.Invoke(this, newState);
            }
        }

        protected override bool GetIsDown(ref KeyboardKeyState state)
        {
            return state.State == InputPressState.Pressed || state.State == InputPressState.Held;
        }

        protected override bool GetIsHeld(ref KeyboardKeyState state)
        {
            return state.State == InputPressState.Held;
        }

        protected override bool GetIsTapped(ref KeyboardKeyState state)
        {
            return state.State == InputPressState.Pressed;
        }

        /// <summary>
        /// Occurs when a character key is pressed.
        /// </summary>
        public event KeyPressHandler OnCharacterKey;

        /// <summary>
        /// Occurs when any type of key is pressed.
        /// </summary>
        public event KeyHandler OnKeyDown;

        /// <summary>
        /// Occurs when a previously-pressed key (of any type) is released.
        /// </summary>
        public event KeyHandler OnKeyUp;

        /// <summary>
        /// Occurs when a previous-down key is held down long enough to trigger another key event.
        /// </summary>
        public event KeyHandler OnKeyHeld;
    }
}

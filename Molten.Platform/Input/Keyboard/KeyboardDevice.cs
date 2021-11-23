namespace Molten.Input
{
    public delegate void KeyHandler(KeyboardDevice device, KeyboardKeyState state);
    public delegate void KeyPressHandler(KeyboardKeyState state);

    public abstract class KeyboardDevice : InputDevice<KeyboardKeyState, KeyCode>
    {
        public KeyboardDevice(InputManager manager) : 
            base(manager, manager.Settings.KeyboardBufferSize)
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
            if (newState.State == InputAction.Held || 
                (newState.State == InputAction.Pressed && prevState.State == InputAction.Pressed))
            {
                newState.State = InputAction.Held;
                newState.PressTimestamp = prevState.PressTimestamp;
                OnKeyHeld?.Invoke(this, newState);
            }else if(newState.State == InputAction.Pressed)
            {
                OnKeyDown?.Invoke(this, newState);
            }else if(newState.State == InputAction.Released)
            {
                if (prevState.State != InputAction.Released && prevState.State != InputAction.None)
                    newState.PressTimestamp = prevState.PressTimestamp;

                OnKeyUp?.Invoke(this, newState);
            }

            if(newState.KeyType == KeyboardKeyType.Character)
            {
                if (newState.State == InputAction.Pressed || newState.State == InputAction.Held)
                    OnCharacterKey?.Invoke(newState);
            }
        }

        protected override bool GetIsDown(ref KeyboardKeyState state)
        {
            return state.State == InputAction.Pressed || state.State == InputAction.Held;
        }

        protected override bool GetIsHeld(ref KeyboardKeyState state)
        {
            return state.State == InputAction.Held;
        }

        protected override bool GetIsTapped(ref KeyboardKeyState state)
        {
            return state.State == InputAction.Pressed;
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

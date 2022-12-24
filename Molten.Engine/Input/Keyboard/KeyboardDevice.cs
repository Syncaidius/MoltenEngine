namespace Molten.Input
{
    public delegate void KeyHandler(KeyboardDevice device, KeyboardKeyState state);
    public delegate void KeyPressHandler(KeyboardDevice device, KeyboardKeyState state);

    public abstract class KeyboardDevice : InputDevice<KeyboardKeyState, KeyCode>
    {
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

        protected override SettingValue<int> GetBufferSizeSetting(InputSettings settings)
        {
            return settings.KeyboardBufferSize;
        }

        protected override StateParameters GetStateParameters()
        {
            return new StateParameters()
            {
                SetCount = 1,
                StatesPerSet = (int)KeyCode.OemClear + 1,
            };
        }

        protected override int TranslateStateID(KeyCode idValue)
        {
            return (int)idValue;
        }

        protected override int GetStateID(ref KeyboardKeyState state)
        {
            return (int)state.Key;
        }

        protected override bool ProcessState(ref KeyboardKeyState newState, ref KeyboardKeyState prevState)
        {
            if (newState.Action == InputAction.Held ||
                (newState.Action == InputAction.Pressed &&
                (prevState.Action == InputAction.Pressed || prevState.Action == InputAction.Held))
                )
            {
                newState.Action = InputAction.Held;
                newState.PressTimestamp = prevState.PressTimestamp;
                OnKeyHeld?.Invoke(this, newState);
                OnKeyDown?.Invoke(this, newState);
            }

            Console.WriteLine($"Key State: {newState.Key} -- Action: {newState.Action} -- Time: {newState.PressTimestamp}");

            if (newState.Action != InputAction.None && prevState.Action != InputAction.None && 
                prevState.Action != InputAction.Released)
            {
                newState.PressTimestamp = prevState.PressTimestamp;
            }

            if(newState.KeyType == KeyboardKeyType.Character)
            {
                if (newState.Action == InputAction.Pressed || 
                    newState.Action == InputAction.Held)
                {
                    OnCharacterKey?.Invoke(this, newState);
                }

                // Don't accept character key events as key states.
                return false;
            }
            else
            {
                if (newState.Action == InputAction.Pressed)
                    OnKeyDown?.Invoke(this, newState);
                else if (newState.Action == InputAction.Released)
                    OnKeyUp?.Invoke(this, newState);
            }

            return true;
        }

        protected override bool GetIsDown(ref KeyboardKeyState state)
        {
            return state.Action == InputAction.Pressed || state.Action == InputAction.Held;
        }

        protected override bool GetIsHeld(ref KeyboardKeyState state)
        {
            return state.Action == InputAction.Held;
        }

        protected override bool GetIsTapped(ref KeyboardKeyState state, InputActionType type)
        {
            return (state.Action == InputAction.Pressed && state.ActionType == type) && state.UpdateID == Service.UpdateID;
        }
    }
}

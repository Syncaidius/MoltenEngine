namespace Molten.Input
{
    public abstract class GamepadDevice : InputDevice<GamepadButtonState, GamepadButtons>
    {
        protected override StateParameters GetStateParameters()
        {
            return new StateParameters()
            {
                SetCount = 1,
                StatesPerSet = (int)GamepadButtons.Y + 1
            };
        }

        protected override SettingValue<int> GetBufferSizeSetting(InputSettings settings)
        {
            return settings.GamepadBufferSize;
        }

        protected override bool ProcessState(ref GamepadButtonState newState, ref GamepadButtonState prevState)
        {
            if (prevState.Action == InputAction.Released)
                newState.PressTimestamp = prevState.PressTimestamp;

            newState.HeldTime = DateTime.UtcNow - newState.PressTimestamp;

            return true;
        }

        protected override int TranslateStateID(GamepadButtons idValue)
        {
            return (int)idValue;
        }

        protected override bool GetIsDown(ref GamepadButtonState state)
        {
            return state.Action == InputAction.Pressed || 
                state.Action == InputAction.Held;
        }

        protected override bool GetIsHeld(ref GamepadButtonState state)
        {
            return state.Action == InputAction.Held;
        }

        protected override bool GetIsTapped(ref GamepadButtonState state)
        {
            return state.Action == InputAction.Pressed;
        }

        protected override int GetStateID(ref GamepadButtonState state)
        {
            return (int)state.Button;
        }

        /// <summary>Gets or sets the vibration level of the left force-feedback motor.</summary>
        public abstract InputVibration VibrationLeft { get; protected set; }

        /// <summary>Gets or sets the vibration level of the right force-feedback motor.</summary>
        public abstract InputVibration VibrationRight { get; protected set; }

        /// <summary>
        /// Gets the X and Y axis values of the left thumbstick.
        /// </summary>
        public abstract InputAnalogStick LeftStick { get; protected set; }

        /// <summary>
        /// Gets the X and Y axis values of the right thumbstick.
        /// </summary>
        public abstract InputAnalogStick RightStick { get; protected set; }

        /// <summary>
        /// Gets the gamepad's left trigger.
        /// </summary>
        public abstract InputAnalogTrigger LeftTrigger { get; protected set; }

        /// <summary>
        /// Gets the gamepad's right trigger.
        /// </summary>
        public abstract InputAnalogTrigger RightTrigger { get; protected set; }

        /// <summary>
        /// Gets the sub-type of the game pad/controller.
        /// </summary>
        public abstract GamepadSubType SubType { get; }

        /// <summary>Gets the index of the gamepad.</summary>
        public int Index { get; set; }
    }
}

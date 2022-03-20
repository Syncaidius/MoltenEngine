namespace Molten.Input
{
    public struct GamepadButtonState : IInputState
    {
        /// <summary>
        /// Gets the game pad button associated with the current <see cref="GamepadButtonState"/>.
        /// </summary>
        public GamepadButton Button;

        /// <summary>
        /// Gets the pressure of the button, if pressed, between 0.0f and 1.0f. 
        /// This may always be 1.0f if the device button is not pressure-sensitive.
        /// </summary>
        public float Pressure;

        /// <summary>
        /// Gets the current button press state.
        /// </summary>
        public InputAction Action;

        /// <summary>
        /// Gets the UTC time at which the button was last pressed.
        /// </summary>
        DateTime _pressTimestamp;
        public DateTime PressTimestamp
        {
            get => _pressTimestamp;
            set => _pressTimestamp = value;
        }

        /// <summary>
        /// Gets the amount of time that the <see cref="GamepadButton"/> has been held.
        /// </summary>
        public TimeSpan HeldTime;

        ulong _updateID;
        public ulong UpdateID
        {
            get => _updateID;
            set => _updateID = value;
        }
    }
}

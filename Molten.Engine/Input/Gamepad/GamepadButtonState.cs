namespace Molten.Input
{
    /// <summary>
    /// Represents a snapshot of a <see cref="GamepadDevice"/> state.
    /// </summary>
    public struct GamepadButtonState : IInputState
    {
        /// <summary>
        /// Gets the game pad button associated with the current <see cref="GamepadButtonState"/>.
        /// </summary>
        public GamepadButtons Button;

        /// <summary>
        /// Gets the Set ID.
        /// </summary>
        public int SetID { get; set; }

        /// <summary>
        /// Gets the pressure of the button, if pressed, between 0.0f and 1.0f. 
        /// This may always be 1.0f if the device button is not pressure-sensitive.
        /// </summary>
        public float Pressure;

        /// <summary>
        /// Gets the current button press state.
        /// </summary>
        public InputAction Action { get; set; }

        /// <summary>
        /// Gets or sets the input action type.
        /// </summary>
        public InputActionType ActionType { get; set; }

        /// <summary>
        /// Gets the UTC time at which the button was last pressed.
        /// </summary>
        public DateTime PressTimestamp { get; set; }

        /// <summary>
        /// Gets the amount of time that the <see cref="GamepadButtons"/> has been held.
        /// </summary>
        public TimeSpan HeldTime;

        /// <summary>
        /// Gets the frame/update ID on which the current <see cref="GamepadButtonState"/> was produced.
        /// </summary>
        public ulong UpdateID { get; set; }
    }
}

namespace Molten.Input
{
    public abstract class GamepadDevice : InputDevice<GamepadButtonFlags>
    {
        public GamepadDevice(IInputManager manager, GamepadIndex index, Logger log) : base(manager, log)
        {
            Index = index;
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
        public GamepadIndex Index { get; }

        /// <summary>
        /// Gets a flags value containing all of the currently-pressed gamepad buttons.
        /// </summary>
        public abstract GamepadButtonFlags PressedButtons { get; }
    }
}

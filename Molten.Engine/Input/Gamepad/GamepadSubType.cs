namespace Molten.Input
{
    public enum GamepadSubType
    {
        Unknown = 0,

        /// <summary>
        /// A gamepad with the same or similar configuration as an Xbox 360 controller.
        /// </summary>
        Gamepad = 1,

        Wheel = 2,

        ArcadeStick = 3,

        FlightStick = 4,

        DancePad = 5,

        Guitar = 6,

        GuitarAlternate = 7,

        DrumKit = 8,

        GuitarBass = 11,

        ArcadePad = 19,

        /// <summary>
        /// A controller with the same or similar configuration as an Xbox One controller. Xbox One controllers have additional
        /// force feedback vibration controllers.
        /// </summary>
        XOnePad = 50,
    }
}

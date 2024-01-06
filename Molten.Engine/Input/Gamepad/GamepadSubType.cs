namespace Molten.Input;

public enum GamepadSubType
{
    Unknown = 0,

    /// <summary>
    /// A gamepad with the same or similar configuration as an Xbox 360 controller.
    /// </summary>
    Gamepad = 0x01,

    Wheel = 0x02,

    ArcadeStick = 0x03,

    FlightStick = 0x04,

    DancePad = 0x05,

    Guitar = 0x06,

    GuitarAlternate = 0x07,

    DrumKit = 0x08,

    GuitarBass = 0x0B,

    ArcadePad = 0x13,

    /// <summary>
    /// A controller with the same or similar configuration as an Xbox One controller. Xbox One controllers have additional
    /// force feedback vibration controllers.
    /// </summary>
    XOnePad = 50,
}

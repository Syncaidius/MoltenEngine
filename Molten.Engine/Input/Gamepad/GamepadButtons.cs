namespace Molten.Input
{
    /// <summary>
    /// Represents one or more gamepad buttons. Enum values are mapped to match X-Input struct XINPUT_GAMEPAD buttons enum.
    /// <para>See: https://docs.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_gamepad for X-Input mapping details.</para>
    /// </summary>
    [Flags]
    public enum GamepadButtons
    {
        None = 0,

        DPadUp = 0x0001,

        DPadDown = 0x0002,

        DPadLeft = 0x0004,

        DPadRight = 0x0008,

        Start = 0x0010,

        Back = 0x0020,

        LeftThumb = 0x0040,

        RightThumb = 0x0080,

        LeftShoulder = 0x0100,

        RightShoulder = 0x0200,

        A = 0x1000,

        B = 0x2000,

        X = 0x4000,

        Y = 0x8000
    }

    public static class GamepadButtonFlagsExtensions
    {
        public static bool HasButtons(this GamepadButtons value, GamepadButtons flags)
        {
            return (value & flags) == flags;
        }
    }
}

using System;

namespace Molten.Input
{

    [Flags]
    public enum GamepadButton
    {
        None = 0,

        DPadUp = 1,

        DPadDown = 2,

        DPadLeft = 3,

        DPadRight = 4,

        Start = 5,

        Back = 6,

        LeftThumb = 7,

        RightThumb = 8,

        LeftShoulder = 9,

        RightShoulder = 10,

        A = 11,

        B = 12,

        X = 13,

        Y = 14,
    }

    public static class GamepadButtonFlagsExtensions
    {
        public static bool HasButtons(this GamepadButton value, GamepadButton flags)
        {
            return (value & flags) == flags;
        }
    }
}

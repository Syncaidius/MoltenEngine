using System;

namespace Molten.Input
{

    [Flags]
    public enum GamepadButtonFlags : short
    {
        Y = short.MinValue,

        None = 0,

        DPadUp = 1,

        DPadDown = 2,

        DPadLeft = 4,

        DPadRight = 8,

        Start = 16,

        Back = 32,

        LeftThumb = 64,

        RightThumb = 128,

        LeftShoulder = 256,

        RightShoulder = 512,

        A = 4096,

        B = 8192,

        X = 16384
    }

    public static class GamepadButtonFlagsExtensions
    {
        public static bool HasButtons(this GamepadButtonFlags value, GamepadButtonFlags flags)
        {
            return (value & flags) == flags;
        }
    }
}

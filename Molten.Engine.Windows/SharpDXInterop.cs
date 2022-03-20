namespace Molten.Input
{
    internal static class SharpDXInterop
    {
        internal static GamepadSubType FromApi(this SharpDX.XInput.DeviceSubType subType)
        {
            return (GamepadSubType)subType;
        }

        internal static GamepadButton FromApi(this SharpDX.XInput.GamepadButtonFlags flags)
        {
            return (GamepadButton)flags;
        }
    }
}

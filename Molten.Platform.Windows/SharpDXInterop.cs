using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

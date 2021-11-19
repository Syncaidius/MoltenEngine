using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    internal static class SharpDXInterop
    {
        internal  static SharpDX.DirectInput.Key ToApi(this KeyCode key)
        {
            return (SharpDX.DirectInput.Key)key;
        }

        internal static KeyCode FromApi(this SharpDX.DirectInput.Key key)
        {
            return (KeyCode)key;
        }

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

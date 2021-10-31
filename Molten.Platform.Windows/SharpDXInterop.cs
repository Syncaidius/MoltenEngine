using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    internal static class SharpDXInterop
    {
        internal  static SharpDX.DirectInput.Key ToApi(this Key key)
        {
            return (SharpDX.DirectInput.Key)key;
        }

        internal static Key FromApi(this SharpDX.DirectInput.Key key)
        {
            return (Key)key;
        }

        internal static GamepadSubType FromApi(this SharpDX.XInput.DeviceSubType subType)
        {
            return (GamepadSubType)subType;
        }

        internal static GamepadButtonFlags FromApi(this SharpDX.XInput.GamepadButtonFlags flags)
        {
            return (GamepadButtonFlags)flags;
        }
    }
}

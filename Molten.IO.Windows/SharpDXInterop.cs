using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    internal static class SharpDXInterop
    {
        public static SharpDX.DirectInput.Key ToApi(this Key key)
        {
            return (SharpDX.DirectInput.Key)key;
        }

        internal static Key FromApi(this SharpDX.DirectInput.Key key)
        {
            return (Key)key;
        }
    }
}

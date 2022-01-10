using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Dxgi
{
    [Flags]
    public enum WindowAssociationFlags : uint
    {
        None = 0U,

        NoWindowChanges = 1U,

        NoAltEnter = 2U,

        NoPrintScreen = 4U,
    }
}

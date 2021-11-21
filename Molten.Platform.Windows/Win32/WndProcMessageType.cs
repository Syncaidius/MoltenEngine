using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Windows32
{
    internal enum WndProcMessageType
    {
        GWL_WNDPROC = -4,
        WM_KEYDOWN = 0x100,
        WM_KEYUP = 0x101,
        WM_CHAR = 0x102,
        WM_IME_SETCONTEXT = 0x0281,
        WM_INPUTLANGCHANGE = 0x51,
        WM_GETDLGCODE = 0x87,
        WM_IME_COMPOSITION = 0x10f,
        DLGC_WANTALLKEYS = 4,
    }
}

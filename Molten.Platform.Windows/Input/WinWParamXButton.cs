using System;

namespace Molten.Input
{
    /// <summary>
    /// See high-order wParam information here: https://docs.microsoft.com/en-us/windows/win32/inputdev/WM-XBUTTONUP
    /// </summary>
    [Flags]
    public enum WinWParamXButton
    {
        None = 0,

        XButton1 = 0x0001,

        XButton2 = 0x0002,
    }
}

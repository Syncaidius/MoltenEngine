using System;
using System.Runtime.InteropServices;

namespace Molten.Windows32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMessage
    {
        public IntPtr handle;
        public uint msg;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Vector2I p;
    }
}

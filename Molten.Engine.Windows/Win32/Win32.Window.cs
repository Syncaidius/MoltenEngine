using System.Runtime.InteropServices;

namespace Molten.Windows32
{
    public delegate void WndProcCallbackHandler(IntPtr windowHandle, WndProcMessageType msgType, uint wParam, int lParam);

    public static partial class Win32
    {        
        public const int DLGC_WANTALLKEYS = 4;

        public static event WndProcCallbackHandler OnWndProcMessage;

        [DllImport("user32.dll", EntryPoint = "PeekMessage")]
        public static extern int PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin,
                                              int wMsgFilterMax, int wRemoveMsg);

        [DllImport("user32.dll", EntryPoint = "GetMessage")]
        public static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin,
                                             int wMsgFilterMax);

        [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
        public static extern int TranslateMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll", EntryPoint = "DispatchMessage")]
        public static extern int DispatchMessage(ref NativeMessage lpMsg);

        public enum WindowLongType : int
        {
            WndProc = (-4),
            HInstance = (-6),
            HwndParent = (-8),
            Style = (-16),
            ExtendedStyle = (-20),
            UserData = (-21),
            Id = (-12)
        }

        public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static IntPtr GetWindowLong(IntPtr hWnd, WindowLongType index)
        {
            if (IntPtr.Size == 4)
                return GetWindowLong32(hWnd, index);

            return GetWindowLong64(hWnd, index);
        }

        //Win32 functions that will be used
        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        /// <summary>Brings a window to the front and makes it the active/focused one.</summary>
        /// <param name="hwnd">The handle/pointer to a window.</param>
        /// <returns></returns>
        [DllImport("User32")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>Gets the currently focused control.</summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetFocus", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLong32(IntPtr hwnd, WindowLongType index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLong64(IntPtr hwnd, WindowLongType index);

        public static IntPtr SetWindowLong(IntPtr hwnd, WindowLongType index, IntPtr wndProcPtr)
        {
            if (IntPtr.Size == 4)
                return SetWindowLong32(hwnd, index, wndProcPtr);
            else
                return SetWindowLongPtr64(hwnd, index, wndProcPtr);
        }

        [DllImport("user32.dll", EntryPoint = "SetParent", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLong32(IntPtr hwnd, WindowLongType index, IntPtr wndProc);

        public static bool ShowWindow(IntPtr hWnd, bool windowVisible)
        {
            return ShowWindow(hWnd, windowVisible ? 1 : 0);
        }

        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Unicode)]
        private static extern bool ShowWindow(IntPtr hWnd, int mCmdShow);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, WindowLongType index, IntPtr wndProc);

        [DllImport("user32.dll", EntryPoint = "CallWindowProc", CharSet = CharSet.Unicode)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        public static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);


        #region Window Hook Management
        private static WndProc _hookProcDelegate = new WndProc(HookProc);
        private static IntPtr _windowHandle = IntPtr.Zero;
        private static IntPtr _wndProc = IntPtr.Zero;
        private static IntPtr _hIMC = IntPtr.Zero;

        public static void HookToWindow(IntPtr winHandle)
        {
            if (winHandle == IntPtr.Zero || winHandle == _windowHandle)
                return;

            if (winHandle == IntPtr.Zero)
            {
                _windowHandle = IntPtr.Zero;
            }
            else
            {
                _windowHandle = winHandle;

                // Update window long for new window
                IntPtr ptrVal = Marshal.GetFunctionPointerForDelegate(_hookProcDelegate);
                _wndProc = SetWindowLong(_windowHandle, WindowLongType.WndProc, ptrVal);
                _hIMC = ImmGetContext(_windowHandle);
            }
        }

        private static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (_wndProc == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr returnCode = CallWindowProc(_wndProc, hWnd, msg, wParam, lParam);
            WndProcMessageType msgType = (WndProcMessageType)msg;
            uint wp = (uint)((long)wParam & uint.MaxValue);

            int lp = IntPtr.Size == 8 ?
                (int)(lParam.ToInt64() & int.MaxValue) :
                lParam.ToInt32();

            switch (msgType)
            {
                case WndProcMessageType.WM_GETDLGCODE:
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
                    break;

                case WndProcMessageType.WM_IME_SETCONTEXT:
                    if (wp == 1)
                        ImmAssociateContext(hWnd, _hIMC);
                    break;

                case WndProcMessageType.WM_INPUTLANGCHANGE:
                    ImmAssociateContext(hWnd, _hIMC);
                    returnCode = (IntPtr)1;
                    break;
            }

            OnWndProcMessage?.Invoke(_windowHandle, msgType, wp, lp);

            return returnCode;
        }
        #endregion
    }
}

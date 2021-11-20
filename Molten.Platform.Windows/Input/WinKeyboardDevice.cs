using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Molten.Graphics;
using Molten.Utilities;

namespace Molten.Input
{
    /// <summary>A handler for keyboard input.</summary>
    public class WinKeyboardDevice : KeyboardDevice
    {
        // TODO detect keyboard device properties: https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-keyboard

        struct ParsedLParam
        {
            public long RepeatCount;
            public long ScanCode;
            public bool ExtendedKey;
            public bool AltKeyPressed;
            public bool PrevPressed;
            public bool Pressed;
        }

        public override string DeviceName => "Windows Keyboard";

        //various Win32 constants that are needed
        const int GWL_WNDPROC = -4;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_CHAR = 0x102;
        const int WM_IME_SETCONTEXT = 0x0281;
        const int WM_INPUTLANGCHANGE = 0x51;
        const int WM_GETDLGCODE = 0x87;
        const int WM_IME_COMPOSITION = 0x10f;
        const int DLGC_WANTALLKEYS = 4;

        //Win32 functions that will be used
        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern long SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern long SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //variables for Win32 stuff
        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        WndProc _hookProcDelegate;
        IntPtr _wndProc;
        IntPtr _hIMC;

        INativeSurface _surface;
        IntPtr _windowHandle;
        bool _bufferUpdated;

        public WinKeyboardDevice(WinInputManager manager, Logger log) :
            base(manager, log)
        {

        }

        protected override List<InputDeviceFeature> Initialize()
        {
            // TODO get extra features
            List<InputDeviceFeature> features = new List<InputDeviceFeature>();
            return features;
        }

        protected override void OnBind(INativeSurface surface)
        {
            _surface = surface;
            SurfaceHandleChanged(surface);
            _surface.OnHandleChanged += SurfaceHandleChanged;
            _surface.OnParentChanged += SurfaceHandleChanged;
            CreateHook();
        }

        protected override void OnUnbind(INativeSurface surface)
        {
            _surface.OnHandleChanged -= SurfaceHandleChanged;
            _surface.OnParentChanged -= SurfaceHandleChanged;
            SetWindowLongDelegate(null);
            _surface = null;
        }

        private void SurfaceHandleChanged(INativeSurface surface)
        {
            if (surface.WindowHandle != null)
            {
                _windowHandle = surface.WindowHandle.Value;
                CreateHook();
            }
        }

        private void CreateHook()
        {
            if (_hookProcDelegate != null || _windowHandle == IntPtr.Zero)
                return;

            _wndProc = IntPtr.Zero;
            _hookProcDelegate = new WndProc(HookProc);

            SetWindowLongDelegate(_hookProcDelegate);
            _hIMC = ImmGetContext(_windowHandle);
        }

        protected override void OnClearState() { }

        private void SetWindowLongDelegate(WndProc hook)
        {
            if (hook != null)
            {
                IntPtr ptrVal = Marshal.GetFunctionPointerForDelegate(hook);

                if (_wndProc == IntPtr.Zero)
                    _wndProc = (IntPtr)SetWindowLongPtr(_windowHandle, GWL_WNDPROC, ptrVal);
            }
        }

        private IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr returnCode = CallWindowProc(_wndProc, hWnd, msg, wParam, lParam);

            switch (msg)
            {
                case WM_GETDLGCODE:
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
                    break;

                case WM_CHAR:
                    IntPtr forewindow = Win32.GetForegroundWindow();
                    if (_windowHandle == forewindow)
                    {
                        KeyboardKeyState state = new KeyboardKeyState()
                        {
                            RawKeyCode = (long)wParam,
                            Key = (KeyCode)wParam,
                            KeyType = ParseKeyType((long)wParam),
                            State = InputPressState.Released
                        };

                        ParsedLParam plp = ParseLParam(ref state, lParam);
                        if (plp.Pressed && plp.PrevPressed)
                        {
                            state.State = InputPressState.Held;
                        }
                        else if (plp.Pressed && !plp.PrevPressed)
                        {
                            state.State = InputPressState.Pressed;
                            state.PressTimestamp = DateTime.UtcNow;
                        }
                        else if (!plp.Pressed && plp.PrevPressed)
                        {
                            state.State = InputPressState.Released;
                        }

                        // TODO Do we queue an extra state for 'ALT' if alt key is pressed?
                        for(int i = 0; i < plp.RepeatCount; i++)
                            QueueState(state);
                    }
                    break;
                case WM_IME_SETCONTEXT:
                    if (wParam.ToInt32() == 1)
                        ImmAssociateContext(hWnd, _hIMC);
                    break;

                case WM_INPUTLANGCHANGE:
                    ImmAssociateContext(hWnd, _hIMC);
                    returnCode = (IntPtr)1;
                    break;
            }

            return returnCode;
        }

        private KeyboardKeyType ParseKeyType(long wmChar)
        {
            KeyCode key = (KeyCode)wmChar;
            switch (key)
            {
                case KeyCode k when (k >= KeyCode.NUM0 && k <= KeyCode.NUM9):
                    return KeyboardKeyType.Character;
                case KeyCode k when (k >= KeyCode.NUMPAD0 && k <= KeyCode.DIVIDE):
                    return KeyboardKeyType.Character;
                case KeyCode k when (k >= KeyCode.A && k <= KeyCode.Z):
                    return KeyboardKeyType.Character;
                case KeyCode k when (k >= KeyCode.OEM_1 && k <= KeyCode.OEM_3):
                    return KeyboardKeyType.Character;
                case KeyCode k when (k >= KeyCode.OEM_4 && k <= KeyCode.OEM_102):
                    return KeyboardKeyType.Character;

                case KeyCode.SPACE:
                    return KeyboardKeyType.Character;

                case KeyCode.LSHIFT:
                case KeyCode.RSHIFT:
                case KeyCode.SHIFT:
                case KeyCode.LCONTROL:
                case KeyCode.RCONTROL:
                case KeyCode.CONTROL:
                case KeyCode.LMENU:
                case KeyCode.RMENU:
                case KeyCode.MENU:
                    return KeyboardKeyType.Modifier;

                default:
                    return KeyboardKeyType.Unknown;
            }
        }

        /// <summary>
        /// Parses the information held in the lParam value. 
        /// See: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-char
        /// </summary>
        /// <param name="state">The state that will hold the parsed information.</param>
        /// <param name="lParam">The raw lParam value.</param>
        private ParsedLParam ParseLParam(ref KeyboardKeyState state, IntPtr lParam)
        {
            long lParamVal = IntPtr.Size == 8 ?
                lParam.ToInt64() :
                lParam.ToInt32();

            return new ParsedLParam()
            {
                RepeatCount = (lParamVal & 0xFFFF),
                ScanCode = ((lParamVal >> 16) & 0xFF),
                ExtendedKey = ((lParamVal >> 24) & 0x01) > 0,
                AltKeyPressed = ((lParamVal >> 29) & 0x01) > 0,
                PrevPressed = ((lParamVal >> 30) & 0x01) > 0,
                Pressed = ((lParamVal >> 31) & 0x01) > 0,
            };
        }

        public override void OpenControlPanel()
        {

        }

        protected override void OnDispose()
        {
            SetWindowLongDelegate(null);
        }

        protected override void OnUpdate(Timing time) { }
    }
}

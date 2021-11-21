using SharpDX.DirectInput;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Molten.Windows32;

namespace Molten.Input
{
    internal delegate void WndProcCallbackHandler(IntPtr windowHandle, WndProcMessageType msgType, long wParam, long lParam);

    public class WinInputManager : InputManager
    {
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

        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        internal event WndProcCallbackHandler OnWndProcMessage;

        // Various Win32 constants that are needed
        const int GWL_WNDPROC = -4;
        const int DLGC_WANTALLKEYS = 4;

        DirectInput _input;
        List<WinGamepadDevice> _gamepads;
        INativeSurface _surface;
        WindowsClipboard _clipboard;
        WndProc _hookProcDelegate;
        IntPtr _windowHandle;
        IntPtr _wndProc;
        IntPtr _hIMC;

        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        protected override void OnInitialize()
        {
            _input = new DirectInput();
            _gamepads = new List<WinGamepadDevice>();
            _clipboard = new WindowsClipboard();
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
            WndProcMessageType msgType = (WndProcMessageType)msg;
            long wp = (long)wParam;

            long lp = IntPtr.Size == 8 ?
                lParam.ToInt64() :
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

        protected override T OnGetCustomDevice<T>()
        {
            return Activator.CreateInstance(typeof(T), args: new object[] { this }) as T;
        }

        protected override void OnBindSurface(INativeSurface surface)
        {
            if(_surface != surface)
            {
                if(_surface != null)
                {
                    _surface.OnHandleChanged -= SurfaceHandleChanged;
                    _surface.OnParentChanged -= SurfaceHandleChanged;
                    SetWindowLongDelegate(null);
                }

                _surface = surface;
                if(_surface != null)
                {
                    SurfaceHandleChanged(surface);
                    _surface.OnHandleChanged += SurfaceHandleChanged;
                    _surface.OnParentChanged += SurfaceHandleChanged;
                    CreateHook();
                }
            }
        }

        private void SurfaceHandleChanged(INativeSurface surface)
        {
            if (surface.WindowHandle != null)
            {
                _windowHandle = surface.WindowHandle.Value;
                CreateHook();
            }
        }

        protected override void OnClearState()
        {

        }

        public override MouseDevice GetMouse()
        {
            return GetCustomDevice<WinMouseDevice>();
        }

        public override KeyboardDevice GetKeyboard()
        {
            return GetCustomDevice<WinKeyboardDevice>();
        }

        public override TouchDevice GetTouch()
        {
            throw new NotImplementedException();
        }

        protected override GamepadDevice OnGetGamepad(int index, GamepadSubType subtype)
        {
            // TODO implement Xbox One controller support: https://github.com/roblambell/XboxOneController/blob/master/XInputInject/Main.cs
            // TODO make use of subtype parameter.

            WinGamepadDevice gp = new WinGamepadDevice(this, index);
            gp.OnDisposing += Gp_OnDisposing;
            _gamepads.Add(gp);
            return gp;
        }

        private void Gp_OnDisposing(EngineObject obj)
        {
            WinGamepadDevice gp = obj as WinGamepadDevice;
            gp.OnDisposing -= Gp_OnDisposing;
            _gamepads.Remove(gp);
        }

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        protected override void OnUpdate(Timing time)
        {
            if (_surface != null)
            {
                for (int i = 0; i < _gamepads.Count; i++)
                    _gamepads[i].Update(time);
            }
        }

        /// <summary>Retrieves a gamepad handler.</summary>
        /// <param name="index">The index of the gamepad.</param>
        /// <returns></returns>
        public WinGamepadDevice GetGamepadHandler(int index)
        {
            return _gamepads[(int)index];
        }

        protected override void OnDispose()
        {
            SetWindowLongDelegate(null);

            _gamepads.Clear();
            DisposeObject(ref _input);
        }

        public DirectInput DirectInput { get { return _input; } }

        public override IClipboard Clipboard => _clipboard;

        public override IInputNavigation Navigation => throw new NotImplementedException();
    }
}

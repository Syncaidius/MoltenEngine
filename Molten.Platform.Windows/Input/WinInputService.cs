using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Molten.Windows32;
using System.Reflection;

namespace Molten.Input
{
    internal delegate void WndProcCallbackHandler(IntPtr windowHandle, WndProcMessageType msgType, int wParam, int lParam);

    // TODO support app commands: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-appcommand

    public class WinInputService : InputService
    {
        internal event WndProcCallbackHandler OnWndProcMessage;

        // Various Win32 constants that are needed
        const int DLGC_WANTALLKEYS = 4;

        List<WinGamepadDevice> _gamepads;
        INativeSurface _surface;
        WindowsClipboard _clipboard;
        Win32.WndProc _hookProcDelegate;
        IntPtr _windowHandle;
        IntPtr _wndProc;
        IntPtr _hIMC;

        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        protected override void OnInitialize(EngineSettings settings, Logger log)
        {
            base.OnInitialize(settings, log);

            _gamepads = new List<WinGamepadDevice>();
            _clipboard = new WindowsClipboard(Thread.Manager);
        }

        private void CreateHook()
        {
            if (_hookProcDelegate != null || _windowHandle == IntPtr.Zero)
                return;

            _wndProc = IntPtr.Zero;
            _hookProcDelegate = new Win32.WndProc(HookProc);

            SetWindowLongDelegate(_hookProcDelegate);
            _hIMC = Win32.ImmGetContext(_windowHandle);
        }

        private void SetWindowLongDelegate(Win32.WndProc hook)
        {
            if (hook != null)
            {
                IntPtr ptrVal = Marshal.GetFunctionPointerForDelegate(hook);

                if (_wndProc == IntPtr.Zero)
                    _wndProc = (IntPtr)Win32.SetWindowLong(_windowHandle, Win32.WindowLongType.WndProc, ptrVal);
            }
        }

        private IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr returnCode = Win32.CallWindowProc(_wndProc, hWnd, msg, wParam, lParam);
            WndProcMessageType msgType = (WndProcMessageType)msg;
            int wp = (int)((long)wParam & int.MaxValue);

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
                        Win32.ImmAssociateContext(hWnd, _hIMC);
                    break;

                case WndProcMessageType.WM_INPUTLANGCHANGE:
                    Win32.ImmAssociateContext(hWnd, _hIMC);
                    returnCode = (IntPtr)1;
                    break;
            }

            OnWndProcMessage?.Invoke(_windowHandle, msgType, wp, lp);

            return returnCode;
        }

        protected override T OnGetCustomDevice<T>()
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            return Activator.CreateInstance(typeof(T), flags, null, args: new object[] { this }, null) as T;
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

            _clipboard.Dispose();
            _gamepads.Clear();
        }

        public override IClipboard Clipboard => _clipboard;

        public override IInputNavigation Navigation => throw new NotImplementedException();
    }
}

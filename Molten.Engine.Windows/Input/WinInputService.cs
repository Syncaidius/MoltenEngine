using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Molten.Windows32;
using System.Reflection;
using Molten.Threading;

namespace Molten.Input
{
    // TODO support app commands: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-appcommand

    public class WinInputService : InputService
    {
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
        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            _gamepads = new List<WinGamepadDevice>();
            _clipboard = new WindowsClipboard();
        }

        protected override ThreadingMode OnStart()
        {
            _clipboard.Start(Thread.Manager);
            return base.OnStart();
        }

        protected override void OnStop()
        {
            _clipboard.Stop();
            base.OnStop();
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
                    Win32.HookToWindow(IntPtr.Zero);
                }

                _surface = surface;
                if(_surface != null)
                {
                    SurfaceHandleChanged(surface);
                    _surface.OnHandleChanged += SurfaceHandleChanged;
                    _surface.OnParentChanged += SurfaceHandleChanged;
                    Win32.HookToWindow(_windowHandle);
                }
            }
        }

        private void SurfaceHandleChanged(INativeSurface surface)
        {
            if (surface.WindowHandle != null)
            {
                _windowHandle = surface.WindowHandle.Value;
                Win32.HookToWindow(_windowHandle);
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
            Win32.HookToWindow(IntPtr.Zero);

            _clipboard.Dispose();
            _gamepads.Clear();
        }

        public override IClipboard Clipboard => _clipboard;

        public override IInputNavigation Navigation => throw new NotImplementedException();
    }
}

using Molten.Graphics;
using Molten.Threading;
using Molten.Windows32;
using System.Reflection;

namespace Molten.Input
{
    // TODO support app commands: https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-appcommand

    public class WinInputService : InputService
    {
        List<WinGamepadDevice> _gamepads;
        INativeSurface _surface;
        WindowsClipboard _clipboard;
        IntPtr _windowHandle;

        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            _gamepads = new List<WinGamepadDevice>();
            _clipboard = new WindowsClipboard();
        }

        protected override ThreadingMode OnStart(ThreadManager threadManager)
        {
            _clipboard.Start(threadManager);
            return base.OnStart(threadManager);
        }

        protected override void OnStop()
        {
            _clipboard.Stop();
            base.OnStop();
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
            return null; // TODO support touch on windows.
        }

        protected override GamepadDevice OnGetGamepad(int index, GamepadSubType subtype)
        {
            // TODO implement Xbox One controller support: https://github.com/roblambell/XboxOneController/blob/master/XInputInject/Main.cs
            // TODO make use of subtype parameter.

            WinGamepadDevice gp = GetCustomDevice<WinGamepadDevice>();
            gp.Index = index;
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
            return _gamepads[index];
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

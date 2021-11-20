using SharpDX.DirectInput;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Input
{
    public class WinInputManager : InputManager
    {
        DirectInput _input;
        List<WinGamepadDevice> _gamepads;
        INativeSurface _activeSurface;
        WindowsClipboard _clipboard;

        /// <summary>Initializes the current input manager instance. Avoid calling this directly unless you know what you are doing.</summary>
        /// <param name="settings">The <see cref="InputSettings"/> that was provided when the engine was instanciated.</param>
        /// <param name="log">A logger.</param>
        protected override void OnInitialize()
        {
            _input = new DirectInput();
            _gamepads = new List<WinGamepadDevice>();
            _clipboard = new WindowsClipboard();
        }

        protected override T OnGetCustomDevice<T>()
        {
            return Activator.CreateInstance(typeof(T), args: new object[] { this }) as T;
        }

        protected override void OnBindSurface(INativeSurface surface)
        {

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
            if (_activeSurface != null)
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
            _gamepads.Clear();
            DisposeObject(ref _input);
        }

        public DirectInput DirectInput { get { return _input; } }

        public override IClipboard Clipboard => _clipboard;

        public override IInputNavigation Navigation => throw new NotImplementedException();
    }
}

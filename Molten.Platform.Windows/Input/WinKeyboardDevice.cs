using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;
using Molten.Graphics;
using Molten.Utilities;

namespace Molten.Input
{
    /// <summary>A handler for keyboard input.</summary>
    public class WinKeyboardDevice : KeyboardDevice
    {
        public override string DeviceName => _keyboard.Information.ProductName;

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

        //handler variables
        Keyboard _keyboard;
        KeyboardState _state;
        KeyboardState _prevState;

        KeyboardUpdate[] _buffer;

        List<KeyCode> _pressedKeys;
        INativeSurface _surface;
        IntPtr _windowHandle;
        bool _bufferUpdated;

        public WinKeyboardDevice(WinInputManager manager, Logger log) : 
            base(manager, manager.Settings.KeyboardBufferSize, og)
        {

        }

        protected override List<InputDeviceFeature> Initialize()
        {
            _state = new KeyboardState();
            _prevState = new KeyboardState();
            _pressedKeys = new List<KeyCode>();

            WinInputManager manager = Manager as WinInputManager;
            _keyboard = new Keyboard(manager.DirectInput);
            _keyboard.Properties.BufferSize = Manager.Settings.KeyboardBufferSize;
            Manager.Settings.KeyboardBufferSize.OnChanged += KeyboardBufferSize_OnChanged;
            _keyboard.Acquire();

            // TODO get extra features
            List<InputDeviceFeature> features = new List<InputDeviceFeature>();
            return features;
        }

        private void KeyboardBufferSize_OnChanged(int oldValue, int newValue)
        {
            _keyboard.Unacquire();
            _keyboard.Properties.BufferSize = newValue;
            _keyboard.Acquire();
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

        protected override void OnClearState()
        {
            _pressedKeys.Clear();
            _state = new KeyboardState();
            _prevState = new KeyboardState();
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

            switch (msg)
            {
                case WM_GETDLGCODE:
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
                    break;

                case WM_CHAR:
                    long paramVal = lParam.ToInt64();
                    OnCharacterKey?.Invoke((char)wParam, paramVal);
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

        public override void OpenControlPanel()
        {
            _keyboard.RunControlPanel();
        }

        /// <summary>Returns true if the given keyboard key is pressed.</summary>
        /// <param name="key">The key to test.</param>
        /// <returns>True if pressed.</returns>
        public override bool IsDown(KeyCode key)
        {
            return _state.IsPressed(key.ToApi());
        }

        /// <summary>Returns true if the key is pressed, but wasn't already pressed previously.</summary>
        /// <param name="key">THe key to test.</param>
        /// <returns>Returns true if the key is pressed, but wasn't already pressed previously.</returns>
        public override bool IsTapped(KeyCode key)
        {
            bool isPressed = _state.IsPressed(key.ToApi());
            bool wasPressed = _prevState.IsPressed(key.ToApi());

            return isPressed == true && wasPressed == false;
        }

        /// <summary>Returns true if the specified key was pressed in both the previous and current frame.</summary>
        /// <param name="key">The key to test.</param>
        /// <returns>True if key(s) considered held.</returns>
        public override bool IsHeld(KeyCode key)
        {
            SharpDX.DirectInput.Key sKey = key.ToApi();
            return _state.IsPressed(sKey) && _prevState.IsPressed(sKey);
        }

        protected override void OnDispose()
        {
            SetWindowLongDelegate(null);
            DisposeObject(ref _keyboard);
            _buffer = null;
        }

        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of game time to use.</param>
        internal override void Update(Timing time)
        {
            // Update previous state with buffer
            if (_buffer != null && _bufferUpdated)
            {
                for (int i = 0; i < _buffer.Length; i++)
                    _prevState.Update(_buffer[i]);
            }

            _bufferUpdated = false;
            if (_windowHandle == IntPtr.Zero)
                return;

            IntPtr forewindow = Win32.GetForegroundWindow();

            // Compare the foreground window to the current engine window.
            if (_windowHandle == forewindow)
            {
                _keyboard.Poll();
                _buffer = _keyboard.GetBufferedData();
                _bufferUpdated = true;

                // Update current state with new buffer data
                if (_buffer != null)
                    for (int i = 0; i < _buffer.Length; i++)
                        _state.Update(_buffer[i]);

                // Handle released keys
                if (OnKeyReleased != null)
                {
                    for (int i = 0; i < _pressedKeys.Count; i++)
                    {
                        KeyCode key = _pressedKeys[i];
                        if (_state.PressedKeys.Contains(key.ToApi()) == false)
                            OnKeyReleased?.Invoke(this, key);
                    }
                }

                //Clear pressed list
                _pressedKeys.Clear();

                // Handle newly pressed keys
                for (int i = 0; i < _state.PressedKeys.Count; i++)
                {
                    KeyCode key = _state.PressedKeys[i].FromApi();
                    _pressedKeys.Add(key);
                    OnKeyPressed?.Invoke(this, key);
                }
            }
            else
            {
                _state.PressedKeys.Clear();
                _pressedKeys.Clear();
            }
        }
    }
}

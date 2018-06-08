using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDX.Multimedia;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;
using SharpDX.Windows;
using Molten.Graphics;
using Molten.Utilities;

namespace Molten.Input
{
    /// <summary>A handler for keyboard input.</summary>
    public class KeyboardDevice : InputHandlerBase<Key>, IKeyboardDevice
    {
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
        public delegate void OnKeyHandler(Key key);

        ReferencedObject<WndProc> _hookProcDelegate;
        IntPtr _prevWndProc;
        IntPtr _hIMC;

        //handler variables
        Keyboard _keyboard;
        KeyboardState _state;
        KeyboardState _prevState;

        KeyboardUpdate[] _buffer;

        List<Key> _pressedKeys;
        IWindowSurface _surface;

        /// <summary>Triggered when a character key is pressed.</summary>
        public event KeyPressHandler OnCharacterKey;
        public event OnKeyHandler OnKeyReleased;

        internal override void Initialize(IInputManager manager, Logger log, IWindowSurface surface)
        {
            InputManager diManager = manager as InputManager;

            _surface = surface;
            _state = new KeyboardState();
            _prevState = new KeyboardState();
            _pressedKeys = new List<Key>();

            _keyboard = new Keyboard(diManager.DirectInput);
            _keyboard.Properties.BufferSize = 256;
            _keyboard.Acquire();            
            CreateHook();
            surface.OnPostResize += Surface_OnPostResize;
        }

        private void Surface_OnPostResize(ITexture texture)
        {
            CreateHook();
        }

        private void CreateHook()
        {
            if (_hookProcDelegate != null || _surface.Handle == IntPtr.Zero)
                return;

            _prevWndProc = IntPtr.Zero;

            //if (_hookProcDelegate == null)
            _hookProcDelegate = new ReferencedObject<WndProc>(new WndProc(HookProc));
            //else
            //    _hookProcDelegate.Reference();

            SetWindowLongDelegate(_hookProcDelegate);
            _hIMC = ImmGetContext(_surface.Handle);
        }

        public override void ClearState()
        {
            _pressedKeys.Clear();
        }

        private void SetWindowLongDelegate(WndProc hook)
        {
            if (hook != null)
            {
                IntPtr ptrVal = Marshal.GetFunctionPointerForDelegate(hook);

                if (_prevWndProc == IntPtr.Zero)
                {
                    if (IntPtr.Size == 8) // 64-bit
                        _prevWndProc = (IntPtr)SetWindowLongPtr(_surface.Handle, GWL_WNDPROC, ptrVal);
                    else 
                        _prevWndProc = (IntPtr)SetWindowLong(_surface.Handle, GWL_WNDPROC, ptrVal.ToInt32());
                }
            }
        }

        private void ResetWindowLong()
        {
            if (_prevWndProc != null)
            {
                if (IntPtr.Size == 8) // 64-bit
                    SetWindowLongPtr(_surface.Handle, GWL_WNDPROC, _prevWndProc);
                else 
                    SetWindowLong(_surface.Handle, GWL_WNDPROC, _prevWndProc.ToInt32());
            }
        }

        private IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr returnCode = CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);

            switch (msg)
            {
                case WM_GETDLGCODE:
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
                    break;

                case WM_CHAR:
                    long paramVal = lParam.ToInt64();

                    CharacterEventArgs e = new CharacterEventArgs((char)wParam, paramVal);
                    OnCharacterKey?.Invoke(e);
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

            // Dispose of hook delegate if the handler was previously disposed.
            // TODO this needs to happen in the handler.Dispose method too, but in a way that doesn't invalidate the hook to the native-side of things.
            if (IsDisposed)
                _hookProcDelegate.Dereference();

            return returnCode;
        }

        public override void OpenControlPanel()
        {
            _keyboard.RunControlPanel();
        }

        /// <summary>Returns true if the given keyboard key is pressed.</summary>
        /// <param name="key">The key to test.</param>
        /// <returns>True if pressed.</returns>
        public override bool IsPressed(Key key)
        {
            return _state.IsPressed(key.ToApi());
        }

        /// <summary>Returns true if the key is pressed, but wasn't already pressed previously.</summary>
        /// <param name="key">THe key to test.</param>
        /// <returns>Returns true if the key is pressed, but wasn't already pressed previously.</returns>
        public override bool IsTapped(Key key)
        {
            bool isPressed = _state.IsPressed(key.ToApi());
            bool wasPressed = _prevState.IsPressed(key.ToApi());

            return isPressed == true && wasPressed == false;
        }

        /// <summary>Returns true if the specified keys have been held for at least the given interval of time.</summary>
        /// <param name="key">The key to test.</param>
        /// <param name="interval">The interval of time the key(s) must be held for to be considered as held.</param>
        /// <param name="reset">Set to true if the current amount of time the button has been held should be reset.</param>
        /// <returns>True if key(s) considered held.</returns>
        public override bool IsHeld(Key key, int interval, bool reset)
        {
            SharpDX.DirectInput.Key sKey = key.ToApi();
            return _state.IsPressed(sKey) && _prevState.IsPressed(sKey);
        }

        protected override void OnDispose()
        {
            ResetWindowLong();
            DisposeObject(ref _keyboard);
            _buffer = null;
        }

        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of game time to use.</param>
        internal override void Update(Timing time)
        {
            // Update previous state with buffer
            if (_buffer != null)
                for (int i = 0; i < _buffer.Length; i++)
                    _prevState.Update(_buffer[i]);

            _keyboard.Poll();
            _buffer = _keyboard.GetBufferedData();

            IntPtr forewindow = Win32.GetForegroundWindow();

            // Get the current game window + foreground window.
            IntPtr winHandle = _surface.Handle;

            // Compare the foreground window to the current engine window.
            if (winHandle == forewindow)
            {
                // Update current state with new buffer data
                if (_buffer != null)
                    for (int i = 0; i < _buffer.Length; i++)
                        _state.Update(_buffer[i]);

                // Handle released keys
                if (OnKeyReleased != null)
                {
                    for (int i = 0; i < _pressedKeys.Count; i++)
                    {
                        Key key = _pressedKeys[i];
                        if (_state.PressedKeys.Contains(key.ToApi()) == false)
                            OnKeyReleased(key);
                    }
                }

                //Clear pressed list
                _pressedKeys.Clear();

                // Handle newly pressed keys
                for (int i = 0; i < _state.PressedKeys.Count; i++)
                {
                    Key key = _state.PressedKeys[i].FromApi();
                    _pressedKeys.Add(key);
                }
            }
            else
            {
                _state.PressedKeys.Clear();
                _pressedKeys.Clear();
            }
        }

        /// <summary>Gets a list of all currently pressed keys.</summary>
        public Key[] PressedKeys
        {
            get { return _pressedKeys.ToArray(); }
        }

        public override bool IsConnected
        {
            get { return true; }
        }

        public override string DeviceName
        {
            get { return _keyboard.Information.ProductName; }
        }
    }
}

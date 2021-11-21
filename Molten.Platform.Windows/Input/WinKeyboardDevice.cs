using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.Graphics;
using Molten.Windows32;

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

        public WinKeyboardDevice(WinInputManager manager) :
            base(manager)
        {

        }

        protected override List<InputDeviceFeature> Initialize()
        {
            var manager = Manager as WinInputManager;
            manager.OnWndProcMessage += Manager_OnWndProcMessage;
            // TODO get extra features
            List<InputDeviceFeature> features = new List<InputDeviceFeature>();
            return features;
        }

        private void Manager_OnWndProcMessage(IntPtr windowHandle, WndProcMessageType msgType, long wParam, long lParam)
        {
            switch (msgType)
            {
                case WndProcMessageType.WM_CHAR:
                    IntPtr forewindow = Win32.GetForegroundWindow();
                    if (windowHandle == forewindow)
                    {
                        KeyboardKeyState state = new KeyboardKeyState()
                        {
                            RawKeyCode = wParam,
                            Key = (KeyCode)wParam,
                            KeyType = ParseKeyType(wParam),
                            State = InputAction.Released
                        };

                        ParsedLParam plp = ParseLParam(ref state, lParam);
                        if (plp.Pressed && plp.PrevPressed)
                        {
                            state.State = InputAction.Held;
                        }
                        else if (plp.Pressed && !plp.PrevPressed)
                        {
                            state.State = InputAction.Pressed;
                            state.PressTimestamp = DateTime.UtcNow;
                        }
                        else if (!plp.Pressed && plp.PrevPressed)
                        {
                            state.State = InputAction.Released;
                        }

                        // TODO Do we queue an extra state for 'ALT' if alt key is pressed?
                        for (int i = 0; i < plp.RepeatCount; i++)
                            QueueState(state);
                    }
                    break;
            }
        }

        protected override void OnBind(INativeSurface surface) { }

        protected override void OnUnbind(INativeSurface surface) { }


        protected override void OnClearState() { }

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
        private ParsedLParam ParseLParam(ref KeyboardKeyState state, long lParam)
        {
            return new ParsedLParam()
            {
                RepeatCount = (lParam & 0xFFFF),
                ScanCode = ((lParam >> 16) & 0xFF),
                ExtendedKey = ((lParam >> 24) & 0x01) > 0,
                AltKeyPressed = ((lParam >> 29) & 0x01) > 0,
                PrevPressed = ((lParam >> 30) & 0x01) > 0,
                Pressed = ((lParam >> 31) & 0x01) > 0,
            };
        }

        public override void OpenControlPanel()
        {

        }

        protected override void OnDispose()
        {
           
        }

        protected override void OnUpdate(Timing time) { }
    }
}

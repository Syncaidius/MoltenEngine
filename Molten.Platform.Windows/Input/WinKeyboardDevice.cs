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
            MaxSimultaneousStates = (int)KeyCode.OemClear;
            manager.OnWndProcMessage += Manager_OnWndProcMessage;

            // TODO get extra features
            List<InputDeviceFeature> features = new List<InputDeviceFeature>();
            return features;
        }

        private void Manager_OnWndProcMessage(IntPtr windowHandle, WndProcMessageType msgType, long wParam, long lParam)
        {
            IntPtr forewindow = Win32.GetForegroundWindow();
            ParsedLParam plp;
            KeyboardKeyState state = new KeyboardKeyState()
            {
                Key = 0,
                KeyType = ParseKeyType(wParam),
                State = InputAction.Pressed,
                Character = char.MinValue
            };

            // TODO implement keyboard messages: https://docs.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input#keystroke-message-flags
            switch (msgType)
            {
                case WndProcMessageType.WM_CHAR:
                    if (windowHandle == forewindow)
                    {
                        state.Key = 0;
                        state.Character = (char)wParam;
                        state.State = InputAction.Pressed;
                        state.KeyType = KeyboardKeyType.Character;

                        plp = ParseLParam(ref state, lParam);

                        // TODO Do we queue an extra state for 'ALT' if alt key is pressed?
                        for (int i = 0; i < plp.RepeatCount; i++)
                            QueueState(state);
                    }
                    break;

                case WndProcMessageType.WM_KEYDOWN:
                case WndProcMessageType.WM_KEYUP:
                    state.KeyType = KeyboardKeyType.Normal;
                    plp = ParseLParam(ref state, lParam);

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

                    for (int i = 0; i < plp.RepeatCount; i++)
                        QueueState(state);
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
                case KeyCode k when (k >= KeyCode.Num0 && k <= KeyCode.Num9):
                    return KeyboardKeyType.Character;
                case KeyCode k when (k >= KeyCode.Numpad0 && k <= KeyCode.Divide):
                    return KeyboardKeyType.Character;
                case KeyCode k when (k >= KeyCode.A && k <= KeyCode.Z):
                    return KeyboardKeyType.Character;
                case KeyCode k when (k >= KeyCode.Oem1 && k <= KeyCode.Oem3):
                    return KeyboardKeyType.Character;
                case KeyCode k when (k >= KeyCode.Oem4 && k <= KeyCode.Oem102):
                    return KeyboardKeyType.Character;

                case KeyCode.Space:
                    return KeyboardKeyType.Character;

                case KeyCode.LShift:
                case KeyCode.RShift:
                case KeyCode.Shift:
                case KeyCode.LControl:
                case KeyCode.RControl:
                case KeyCode.Control:
                case KeyCode.LMenu:
                case KeyCode.RMenu:
                case KeyCode.Menu:
                    return KeyboardKeyType.Modifier;

                default:
                    return KeyboardKeyType.Normal;
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
                ExtendedKey = ((lParam >> 24) & 0x01) == 1,
                AltKeyPressed = ((lParam >> 29) & 0x01) == 1,
                PrevPressed = ((lParam >> 30) & 0x01) == 1,
                Pressed = ((lParam >> 31) & 0x01) == 0,
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

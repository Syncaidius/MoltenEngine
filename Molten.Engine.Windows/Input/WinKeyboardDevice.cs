using Molten.Graphics;
using Molten.Windows32;

namespace Molten.Input;

/// <summary>A handler for keyboard input.</summary>
public class WinKeyboardDevice : KeyboardDevice
{
    // TODO Detect keyboard device properties: https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-keyboard

    struct ParsedLParam
    {
        public int RepeatCount;
        public int ScanCode;
        public bool ExtendedKey;
        public bool AltKeyPressed;
        public bool PrevPressed;
        public bool Pressed;
    }

    /// <inheritdoc/>
    public override string DeviceName => "Windows Keyboard";

    /// <inheritdoc/>
    protected override List<InputDeviceFeature> OnInitialize(InputService service)
    {
        List<InputDeviceFeature> baseFeatures = base.OnInitialize(service);

        Win32.OnWndProcMessage += Manager_OnWndProcMessage;

        // TODO get extra keyboard features. e.g. lights, screens, macro buttons.
        List<InputDeviceFeature> features = new List<InputDeviceFeature>();

        if(baseFeatures != null)
            features.AddRange(baseFeatures);

        return features;
    }

    private void Manager_OnWndProcMessage(IntPtr windowHandle, WndMessageType msgType, uint wParam, int lParam)
    {
        if (!IsEnabled)
            return;

        IntPtr forewindow = Win32.GetForegroundWindow();
        KeyboardKeyState state = new KeyboardKeyState()
        {
            Key = 0,
            SetID = 0, // Keyboard only has one set of keys, always at ID 0.
            KeyType = KeyboardKeyType.Normal,
            Action = InputAction.Pressed,
            ActionType = InputActionType.Single,
            Character = char.MinValue,
        };

        if (windowHandle == forewindow)
        {
            switch (msgType)
            {
                case WndMessageType.WM_CHAR:
                    if (windowHandle == forewindow)
                    {
                        state.Key = 0;
                        state.Character = (char)wParam;
                        state.Action = InputAction.Pressed;
                        state.KeyType = KeyboardKeyType.Character;
                        QueueKeyState(ref state, lParam);
                    }
                    break;

                case WndMessageType.WM_KEYDOWN:
                    state.Key = (KeyCode)(wParam & 0xFFFF);
                    state.KeyType = ValidateKeyType(wParam);
                    state.Action = InputAction.Pressed;
                    state.PressTimestamp = DateTime.UtcNow;
                    QueueKeyState(ref state, lParam);
                    break;

                case WndMessageType.WM_KEYUP:
                    state.Key = (KeyCode)(wParam & 0xFFFF);
                    state.KeyType = ValidateKeyType(wParam);
                    state.PressTimestamp = DateTime.UtcNow;
                    state.Action = InputAction.Released;

                    QueueKeyState(ref state, lParam);
                    break;
            }
        }

        // TODO implement keyboard messages: https://docs.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input#keystroke-message-flags
    }

    private void QueueKeyState(ref KeyboardKeyState state, int lParam)
    {
        ParsedLParam plp = ParseLParam(ref state, lParam);

        for (int i = 0; i < plp.RepeatCount; i++)
            QueueState(state);
    }

    /// <inheritdoc/>
    protected override void OnBind(INativeSurface surface) { }

    /// <inheritdoc/>
    protected override void OnUnbind(INativeSurface surface) { }

    /// <inheritdoc/>
    protected override void OnClearState() { }

    private KeyboardKeyType ValidateKeyType(long wmChar)
    {
        KeyCode key = (KeyCode)wmChar;
        switch (key)
        {
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
    private ParsedLParam ParseLParam(ref KeyboardKeyState state, int lParam)
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

    protected override void OnUpdate(Timing time) { }
}

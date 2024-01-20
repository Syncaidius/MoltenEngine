using Molten.Windows32;

namespace Molten.Input;

/// <summary>Handles mouse input.</summary>
public class WinMouseDevice : MouseDevice
{
    /// <summary>
    /// A constant defined in the Win32 API, representing the multiple for calculating wheel deltas.
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/WM-MOUSEWHEEL
    /// </summary>
    const int WHEEL_DELTA = 120;

    /// <inheritdoc/>
    public override string DeviceName => "Windows Mouse";

    static WinMouseButtonFlags[] _winButtons = new WinMouseButtonFlags[]
    {
        WinMouseButtonFlags.MK_LBUTTON,
        WinMouseButtonFlags.MK_MBUTTON,
        WinMouseButtonFlags.MK_RBUTTON,
        WinMouseButtonFlags.MK_XBUTTON1,
        WinMouseButtonFlags.MK_XBUTTON2
    };

    bool _requestedVisibility = true;
    bool _cursorVisibility = true;

    protected override StateParameters GetStateParameters()
    {
        return new StateParameters()
        {
            SetCount = 1,
            StatesPerSet = (int)PointerButton.XButton2 + 1,
        };
    }

    private void CheckConnectivity()
    {
        int result = Win32.GetSystemMetrics(Win32.SM_MOUSEPRESENT);
        IsConnected = result > 0;
    }

    protected override List<InputDeviceFeature> OnInitialize(InputService service)
    {
        // TODO Check if mouse is connected: https://docs.microsoft.com/en-us/windows/win32/inputdev/about-mouse-input#mouse-configuration
        // TODO Check available buttons:
        //      -- An application can determine the number of buttons on the mouse by
        //         passing the SM_CMOUSEBUTTONS value to the GetSystemMetrics function.
        // TODO Consider system scroll settings: https://docs.microsoft.com/en-us/windows/win32/inputdev/about-mouse-input#determining-the-number-of-scroll-lines
        // TODO detect if mouse wheel is present: https://docs.microsoft.com/en-us/windows/win32/inputdev/about-mouse-input#detecting-a-mouse-with-a-wheel

        List<InputDeviceFeature> baseFeatures = base.OnInitialize(service);

        Win32.OnWndProcMessage += Manager_OnWndProcMessage;
        ScrollWheel = new InputScrollWheel("Vertical");
        HScrollWheel = new InputScrollWheel("Horizontal");

        // TODO detect mouse features.
        List< InputDeviceFeature> features = new List<InputDeviceFeature>()
        {
            ScrollWheel,
            HScrollWheel
        };

        if (baseFeatures != null)
            features.AddRange(baseFeatures);

        return features;
    }

    private void Manager_OnWndProcMessage(IntPtr windowHandle, WndMessageType msgType, uint wParam, int lParam)
    {
        if (!IsEnabled)
            return;

        // See: https://docs.microsoft.com/en-us/windows/win32/inputdev/using-mouse-input
        // Positional mouse messages.

        switch (msgType)
        {
            case WndMessageType.WM_MOUSEACTIVATE:

                return;

            case WndMessageType.WM_MOUSEHOVER:
                ParseButtonMessage(WinMouseButtonFlags.None, InputAction.Hover,
                    InputActionType.None, wParam, lParam);
                return;

            case WndMessageType.WM_MOUSEMOVE:
                ParseButtonMessage(WinMouseButtonFlags.None, InputAction.Moved,
                    InputActionType.None, wParam, lParam);
                return;

            case WndMessageType.WM_MOUSEWHEEL:
                ParseButtonMessage(WinMouseButtonFlags.None, InputAction.VerticalScroll,
                    InputActionType.None, wParam, lParam);
                return;

            case WndMessageType.WM_MOUSEHWHEEL:
                ParseButtonMessage(WinMouseButtonFlags.None, InputAction.HorizontalScroll,
                    InputActionType.None, wParam, lParam);
                return;

            case WndMessageType.WM_LBUTTONDOWN:
                ParseButtonMessage(WinMouseButtonFlags.MK_LBUTTON, InputAction.Pressed,
                    InputActionType.Single, wParam, lParam);
                return;

            case WndMessageType.WM_LBUTTONUP:
                ParseButtonMessage(WinMouseButtonFlags.MK_LBUTTON, InputAction.Released,
                    InputActionType.Single, wParam, lParam);
                return;

            case WndMessageType.WM_LBUTTONDBLCLK:
                ParseButtonMessage(WinMouseButtonFlags.MK_LBUTTON, InputAction.Pressed,
                    InputActionType.Double, wParam, lParam);
                return;

            case WndMessageType.WM_MBUTTONDOWN:
                ParseButtonMessage(WinMouseButtonFlags.MK_MBUTTON, InputAction.Pressed,
                    InputActionType.Single, wParam, lParam);
                return;

            case WndMessageType.WM_MBUTTONUP:
                ParseButtonMessage(WinMouseButtonFlags.MK_MBUTTON, InputAction.Released,
                    InputActionType.Single, wParam, lParam);
                return;

            case WndMessageType.WM_MBUTTONDBLCLK:
                ParseButtonMessage(WinMouseButtonFlags.MK_MBUTTON, InputAction.Pressed,
                    InputActionType.Double, wParam, lParam);
                return;

            case WndMessageType.WM_RBUTTONDOWN:
                ParseButtonMessage(WinMouseButtonFlags.MK_RBUTTON, InputAction.Pressed,
                    InputActionType.Single, wParam, lParam);
                return;

            case WndMessageType.WM_RBUTTONUP:
                ParseButtonMessage(WinMouseButtonFlags.MK_RBUTTON, InputAction.Released,
                    InputActionType.Single, wParam, lParam);
                return;

            case WndMessageType.WM_RBUTTONDBLCLK:
                ParseButtonMessage(WinMouseButtonFlags.MK_RBUTTON, InputAction.Pressed,
                    InputActionType.Double, wParam, lParam);
                return;

            case WndMessageType.WM_XBUTTONDOWN:
                ParseButtonMessage(ParseXButton(wParam), InputAction.Pressed,
                    InputActionType.Single, wParam, lParam);
                return;

            case WndMessageType.WM_XBUTTONUP:
                ParseButtonMessage(ParseXButton(wParam), InputAction.Released,
                    InputActionType.Single, wParam, lParam);
                return;

            case WndMessageType.WM_XBUTTONDBLCLK:
                ParseButtonMessage(ParseXButton(wParam), InputAction.Pressed,
                    InputActionType.Double, wParam, lParam);
                return;
        }
    }

    /// <summary>
    /// Parses a message representing a mouse cursor action with coordinates attached to it.
    /// </summary>
    /// <param name="btn">The button that triggered the message.</param>
    /// <param name="action">The action performed</param>
    /// <param name="aType">The type of action.</param>
    /// <param name="wParam">The WndProc wParam value.</param>
    /// <param name="lParam">The WndProc lParam value.</param>
    private void ParseButtonMessage(
        WinMouseButtonFlags btn,
        InputAction action,
        InputActionType aType,
        uint wParam,
        int lParam)
    {
        WinMouseButtonFlags btns = (WinMouseButtonFlags)(wParam & 0xFFFFFFFF);
        PointerState state = new PointerState();
        state.SetID = 0; // Mouse only has one set of buttons, at ID 0.

        state.Position = new Vector2F()
        {
            X = lParam & 0xFFFF,
            Y = (lParam >> 16) & 0xFFFF,
        };

        // Figure out which other buttons are down and queue 'held' states for them.
        if (action == InputAction.Moved)
        {
            foreach (WinMouseButtonFlags b in _winButtons)
            {
                if (b != btn && (btns & b) == b)
                {
                    state.Button = TranslateButton(b);
                    state.Action = InputAction.Held;
                    state.ActionType = InputActionType.None;
                    QueueState(state);
                }
            }
        }

        // Prepare state for button that triggered the current message.
        state.ActionType = aType;
        state.Action = action;
        state.PressTimestamp = DateTime.UtcNow;
        state.Button = TranslateButton(btn);

        switch (action)
        {
            case InputAction.VerticalScroll:
                state.Delta.Y = ParseWheelDelta(wParam);
                break;

            case InputAction.HorizontalScroll:
                state.Delta.X = ParseWheelDelta(wParam);
                break;
        }

        QueueState(state);
    }

    private WinMouseButtonFlags ParseXButton(uint wParam)
    {
        WinWParamXButton xb = (WinWParamXButton)((wParam >> 16) & 0xFFFFFFFF);
        switch (xb)
        {
            default:
            case WinWParamXButton.None:
                return WinMouseButtonFlags.None;

            case WinWParamXButton.XButton1: return WinMouseButtonFlags.MK_XBUTTON1;
            case WinWParamXButton.XButton2: return WinMouseButtonFlags.MK_XBUTTON2;
        }
    }

    private int ParseWheelDelta(uint wParam)
    {
        // Divide by the Windows baseline scroll delta to give us more realistic deltas.
        return ((int)(wParam & 0xFFFF0000U) >> 16) / WHEEL_DELTA;
    }

    private PointerButton TranslateButton(WinMouseButtonFlags btn)
    {
        switch (btn)
        {
            default:
            case WinMouseButtonFlags.None:
            case WinMouseButtonFlags.MK_SHIFT:
            case WinMouseButtonFlags.MK_CONTROL:
                return PointerButton.None;

            case WinMouseButtonFlags.MK_LBUTTON:
                return PointerButton.Left;

            case WinMouseButtonFlags.MK_MBUTTON:
                return PointerButton.Middle;

            case WinMouseButtonFlags.MK_RBUTTON:
                return PointerButton.Right;

            case WinMouseButtonFlags.MK_XBUTTON1:
                return PointerButton.XButton1;

            case WinMouseButtonFlags.MK_XBUTTON2:
                return PointerButton.XButton2;
        }
    }

    public override void OpenControlPanel() { }


    /// <summary>Update input handler.</summary>
    /// <param name="time">The snapshot of game time to use.</param>
    protected override void OnUpdate(Timing time)
    {
        CheckConnectivity();

        if (IsEnabled)
        {
            if (_cursorVisibility != _requestedVisibility)
            {
                // Safely handles the cursor's visibility state, since calls to show and hide are counted. 
                // Two calls to .Show and a last call to .Hide will not hide the cursor.
                if (_requestedVisibility)
                    Cursor.Show();
                else
                    Cursor.Hide();

                _cursorVisibility = _requestedVisibility;
            }
        }
        else
        {
            // Hide mouse when disabled.
            if (_cursorVisibility)
            {
                Cursor.Hide();
                _cursorVisibility = false;
            }
        }
    }

    protected override void OnClearState()
    {
        if (!_cursorVisibility)
        {
            Cursor.Show();
            _cursorVisibility = true;
            _requestedVisibility = true;
        }
    }

    protected override void OnSetCursorVisibility(bool visible)
    {
        _requestedVisibility = visible;
    }

    protected override void OnSetPointerPosition(Vector2F position)
    {
        Cursor.Position = new Point((int)position.X, (int)position.Y);
    }

    protected override void OnDispose(bool immediate)
    {
        base.OnDispose(immediate);

        if (!IsCursorVisible)
            Cursor.Show();
    }

    public override InputScrollWheel ScrollWheel { get; protected set; }

    public override InputScrollWheel HScrollWheel { get; protected set; }
}

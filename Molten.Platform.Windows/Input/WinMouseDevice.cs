using System;
using System.Collections.Generic;
using Molten.Graphics;
using System.Windows.Forms;
using Molten.Windows32;

namespace Molten.Input
{
    /// <summary>Handles mouse input.</summary>
    public class WinMouseDevice : MouseDevice
    {
        /// <summary>
        /// A constant defined in the Win32 API, representing the multiple for calculating wheel deltas.
        /// Ref: https://docs.microsoft.com/en-us/windows/win32/inputdev/WM-MOUSEWHEEL
        /// </summary>
        const int WHEEL_DELTA = 120;

        public override string DeviceName => "Windows Mouse";

        static WinMouseButtonFlags[] _winButtons = new WinMouseButtonFlags[]
        {
            WinMouseButtonFlags.MK_LBUTTON,
            WinMouseButtonFlags.MK_MBUTTON,
            WinMouseButtonFlags.MK_RBUTTON,
            WinMouseButtonFlags.MK_XBUTTON1,
            WinMouseButtonFlags.MK_XBUTTON2
        };

        bool _wasInsideControl = false;
        bool _requestedVisibility = true;
        bool _cursorVisibleState = true;

        public WinMouseDevice(WinInputManager manager) : base(manager)
        {
            // TODO Check if mouse is connected: https://docs.microsoft.com/en-us/windows/win32/inputdev/about-mouse-input#mouse-configuration
            // TODO Check available buttons:
            //      -- An application can determine the number of buttons on the mouse by
            //         passing the SM_CMOUSEBUTTONS value to the GetSystemMetrics function.
            // TODO Consider system scroll settings: https://docs.microsoft.com/en-us/windows/win32/inputdev/about-mouse-input#determining-the-number-of-scroll-lines
            // TODO detect if mouse wheel is present: https://docs.microsoft.com/en-us/windows/win32/inputdev/about-mouse-input#detecting-a-mouse-with-a-wheel
        }

        protected override List<InputDeviceFeature> Initialize()
        {
            WinInputManager manager = Manager as WinInputManager;
            manager.OnWndProcMessage += Manager_OnWndProcMessage;
            ScrollWheel = new InputScrollWheel("Vertical");
            HScrollWheel = new InputScrollWheel("Horizontal");

            // TODO detect mouse features.
            return new List<InputDeviceFeature>()
            {
                ScrollWheel,
                HScrollWheel
            };
        }

        private void Manager_OnWndProcMessage(IntPtr windowHandle, WndProcMessageType msgType, long wParam, long lParam)
        {
            // See: https://docs.microsoft.com/en-us/windows/win32/inputdev/using-mouse-input

            // Positional mouse messages.

            switch (msgType)
            {
                case WndProcMessageType.WM_MOUSEACTIVATE:

                    return;

                case WndProcMessageType.WM_MOUSEHOVER:
                    ParseButtonMessage(WinMouseButtonFlags.None, InputAction.Hover,
                        InputActionType.None, wParam, lParam);
                    return;

                case WndProcMessageType.WM_MOUSEMOVE:
                    ParseButtonMessage(WinMouseButtonFlags.None, InputAction.Released,
                        InputActionType.None, wParam, lParam);
                    return;

                case WndProcMessageType.WM_MOUSEWHEEL:
                    ParseButtonMessage(WinMouseButtonFlags.None, InputAction.VerticalScroll,
                        InputActionType.None, wParam, lParam);
                    return;

                case WndProcMessageType.WM_MOUSEHWHEEL:
                    ParseButtonMessage(WinMouseButtonFlags.None, InputAction.HorizontalScroll,
                        InputActionType.None, wParam, lParam);
                    return;

                case WndProcMessageType.WM_LBUTTONDOWN:
                    ParseButtonMessage(WinMouseButtonFlags.MK_LBUTTON, InputAction.Pressed,
                        InputActionType.Single, wParam, lParam);
                    return;

                case WndProcMessageType.WM_LBUTTONUP:
                    ParseButtonMessage(WinMouseButtonFlags.MK_LBUTTON, InputAction.Released,
                        InputActionType.Single, wParam, lParam);
                    return;

                case WndProcMessageType.WM_LBUTTONDBLCLK:
                    ParseButtonMessage(WinMouseButtonFlags.MK_LBUTTON, InputAction.Pressed,
                        InputActionType.Double, wParam, lParam);
                    return;

                case WndProcMessageType.WM_MBUTTONDOWN:
                    ParseButtonMessage(WinMouseButtonFlags.MK_MBUTTON, InputAction.Pressed,
                        InputActionType.Single, wParam, lParam);
                    return;

                case WndProcMessageType.WM_MBUTTONUP:
                    ParseButtonMessage(WinMouseButtonFlags.MK_MBUTTON, InputAction.Released,
                        InputActionType.Single, wParam, lParam);
                    return;

                case WndProcMessageType.WM_MBUTTONDBLCLK:
                    ParseButtonMessage(WinMouseButtonFlags.MK_MBUTTON, InputAction.Pressed,
                        InputActionType.Double, wParam, lParam);
                    return;

                case WndProcMessageType.WM_RBUTTONDOWN:
                    ParseButtonMessage(WinMouseButtonFlags.MK_RBUTTON, InputAction.Pressed,
                        InputActionType.Single, wParam, lParam);
                    return;

                case WndProcMessageType.WM_RBUTTONUP:
                    ParseButtonMessage(WinMouseButtonFlags.MK_RBUTTON, InputAction.Released,
                        InputActionType.Single, wParam, lParam);
                    return;

                case WndProcMessageType.WM_RBUTTONDBLCLK:
                    ParseButtonMessage(WinMouseButtonFlags.MK_RBUTTON, InputAction.Pressed,
                        InputActionType.Double, wParam, lParam);
                    return;

                case WndProcMessageType.WM_XBUTTONDOWN:
                    ParseButtonMessage(ParseXButton(wParam), InputAction.Pressed,
                        InputActionType.Single, wParam, lParam);
                    return;

                case WndProcMessageType.WM_XBUTTONUP:
                    ParseButtonMessage(ParseXButton(wParam), InputAction.Released,
                        InputActionType.Single, wParam, lParam);
                    return;

                case WndProcMessageType.WM_XBUTTONDBLCLK:
                    ParseButtonMessage(ParseXButton(wParam), InputAction.Pressed,
                        InputActionType.Double, wParam, lParam);
                    return;
            }
        }

        /// <summary>
        /// Parses a message representing a mouse cursor action with coordinates attached to it.
        /// </summary>
        /// <param name="state">The <see cref="MouseButtonState"/> to update.</param>
        /// <param name="btn">The button that triggered the message.</param>
        /// <param name="down">True if the button was pressed down. False if the button was released.</param>
        /// <param name="action">The action performed</param>
        /// <param name="aType">The type of action.</param>
        /// <param name="wParam">The WndProc wParam value.</param>
        /// <param name="lParam">The WndProc lParam value.</param>
        private void ParseButtonMessage(
            WinMouseButtonFlags btn,
            InputAction action,
            InputActionType aType,
            long wParam,
            long lParam)
        {
            WinMouseButtonFlags btns = (WinMouseButtonFlags)(wParam & 0xFFFFFFFF);
            MouseButtonState state = new MouseButtonState();
            state.Position = new Vector2I(lParam);

            // Figure out which other buttons are down and queue 'held' states for them.
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

            // Prepare state for button that triggered the current message.
            if (btn != WinMouseButtonFlags.None)
            {
                state.ActionType = aType;
                state.Button = TranslateButton(btn);
                state.Action = action;
                state.PressTimestamp = DateTime.UtcNow;

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
        }

        private WinMouseButtonFlags ParseXButton(long wParam)
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

        private int ParseWheelDelta(long wParam)
        {
            return (int)((wParam >> 16) & 0xFFFFFFFF);
        }

        private MouseButton TranslateButton(WinMouseButtonFlags btn)
        {
            switch (btn)
            {
                default:
                case WinMouseButtonFlags.None:
                case WinMouseButtonFlags.MK_SHIFT:
                case WinMouseButtonFlags.MK_CONTROL:
                    return MouseButton.None;

                case WinMouseButtonFlags.MK_LBUTTON:
                    return MouseButton.Left;

                case WinMouseButtonFlags.MK_MBUTTON:
                    return MouseButton.Middle;

                case WinMouseButtonFlags.MK_RBUTTON:
                    return MouseButton.Right;

                case WinMouseButtonFlags.MK_XBUTTON1:
                    return MouseButton.XButton1;

                case WinMouseButtonFlags.MK_XBUTTON2:
                    return MouseButton.XButton2;

            }
        }

        protected override void OnBind(INativeSurface surface) { }

        protected override void OnUnbind(INativeSurface surface) { }

        public override void OpenControlPanel() { }


        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of game time to use.</param>
        protected override void OnUpdate(Timing time)
        {
            if (_surface == null)
                return;

            IntPtr forewindow = Win32.GetForegroundWindow();
            Rectangle winBounds = _surface.Bounds;

            _bufferUpdated = false;
            _moved = new Vector2I();
            _wheelDelta = 0f;
            _state.Z = 0;

            // Store previous position
            _prevPosition = _position;
            _prevWheelPos = _wheelPos;

            // Make sure the game window is focused before updating movement/position.
            if (forewindow == _windowHandle)
            {
                // The windows cursor position
                System.Drawing.Point winPos = Cursor.Position;

                // Check if the cursor has gone outside of the control/window 
                bool insideControl = true;
                if (winPos.X < winBounds.Left)
                    insideControl = false;
                else if (winPos.Y < winBounds.Top)
                    insideControl = false;
                else if (winPos.X > winBounds.Right)
                    insideControl = false;
                else if (winPos.Y > winBounds.Bottom)
                    insideControl = false;

                CheckInside(insideControl);

                // If the mouse is in a valid window, process movement, position, etc
                if (insideControl || IsConstrained)
                {
                    // Send all buffered updates to mouse state
                    /*for (int i = 0; i < _pollBuffer.Length; i++)
                    {
                        _state.Update(_pollBuffer[i]);
                        _wheelDelta += _state.Z;
                    }*/

                    System.Drawing.Point osPos = Cursor.Position;
                    Vector2I newPos = new Vector2I(osPos.X, osPos.Y);
                    _moved = newPos - _position;
                    _position = newPos;
                    _wheelPos += _wheelDelta;

                    if (IsConstrained)
                    {
                        if (_position.X < winBounds.X)
                            _position.X = winBounds.X;
                        if (_position.Y < winBounds.Y)
                            _position.Y = winBounds.Y;
                        if (_position.X > winBounds.Width)
                            _position.X = winBounds.Width;
                        if (_position.Y > winBounds.Height)
                            _position.Y = winBounds.Height;
                    }

                    // Update cursor visibility
                    SetCursorVisiblity(_requestedVisibility);
                }
                else
                {
                    _position = new Vector2I(winPos.X, winPos.Y);
                    _moved = new Vector2I();
                    SetCursorVisiblity(true);
                }
            }
            else
            {
                CheckInside(false);
                _moved = Vector2I.Zero;
                SetCursorVisiblity(true);
            }
        }

        private void CheckInside(bool insideControl)
        {
            if (insideControl && !_wasInsideControl)
                OnEnterSurface?.Invoke(this);
            else if (!insideControl && _wasInsideControl)
                OnLeaveSurface?.Invoke(this);

            _wasInsideControl = insideControl;
        }

        /// <summary>Safely handles the cursor's visibility state, since calls to show and hide are counted. 
        /// Two calls to .Show and a last call to .Hide will not hide the cursor.</summary>
        /// <param name="visible">If true, the cursor will be made visible, if not already.</param>
        private void SetCursorVisiblity(bool visible)
        {
            if (_cursorVisibleState == visible)
                return;

            if (visible)
                Cursor.Show();
            else
                Cursor.Hide();

            _cursorVisibleState = visible;
        }

        protected override void OnSetCursorPosition(Vector2I absolute, Vector2I relative)
        {
            Cursor.Position = new System.Drawing.Point(absolute.X, absolute.Y);
        }

        protected override void OnDispose()
        {
            SetCursorVisiblity(true);

        }

        /// <summary>Gets or sets whether or not the mouse cursor is visible.</summary>
        public override bool CursorVisible
        {
            get => _requestedVisibility;
            set => _requestedVisibility = value;
        }

        public override InputScrollWheel ScrollWheel { get; protected set; }

        public override InputScrollWheel HScrollWheel { get; protected set; }
    }
}

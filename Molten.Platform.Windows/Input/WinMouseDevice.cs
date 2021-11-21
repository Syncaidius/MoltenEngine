using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectInput;
using Molten.Graphics;
using System.Windows.Forms;
using Molten.Windows32;

namespace Molten.Input
{
    /// <summary>Handles mouse input.</summary>
    public class WinMouseDevice : MouseDevice
    {
        Mouse _mouse;
        MouseState _state;
        MouseState _prevState;

        Vector2I _position;
        Vector2I _prevPosition;
        Vector2I _moved;

        float _wheelPos;
        float _prevWheelPos;
        float _wheelDelta;

        bool _wasInsideControl = false;
        bool _requestedVisibility = true;
        bool _cursorVisibleState = true;
        INativeSurface _surface;
        IntPtr _windowHandle;
        bool _bufferUpdated;
        MouseUpdate[] _pollBuffer;

        public WinMouseDevice(WinInputManager manager) : base(manager)
        {

        }

        protected override List<InputDeviceFeature> Initialize()
        {
            WinInputManager manager = Manager as WinInputManager;
            manager.OnWndProcMessage += Manager_OnWndProcMessage;

            _mouse = new Mouse(manager.DirectInput);
            _mouse.Properties.AxisMode = DeviceAxisMode.Relative;
            _mouse.Properties.BufferSize = manager.Settings.MouseBufferSize;
            manager.Settings.MouseBufferSize.OnChanged += MouseBufferSize_OnChanged;
            _mouse.Acquire();

            _state = new MouseState();
            _prevState = new MouseState();

            // TODO detect mouse features.
            return new List<InputDeviceFeature>();
        }

        private void Manager_OnWndProcMessage(IntPtr windowHandle, Windows32.WndProcMessageType msgType, long wParam, long lParam)
        {
            // See: https://docs.microsoft.com/en-us/windows/win32/inputdev/using-mouse-input

            MouseButtonState state = new MouseButtonState()
            {

            };

            // Positional mouse messages.
            switch (msgType)
            {
                case WndProcMessageType.WM_MOUSEACTIVATE:

                    break;

                case WndProcMessageType.WM_MOUSEHOVER:

                    break;

                case WndProcMessageType.WM_MOUSEMOVE:

                    break;

                case WndProcMessageType.WM_MOUSEWHEEL:

                    break;

                case WndProcMessageType.WM_MOUSEHWHEEL:

                    break;

                case WndProcMessageType.WM_LBUTTONDOWN:

                    break;

                case WndProcMessageType.WM_LBUTTONUP:

                    break;

                case WndProcMessageType.WM_LBUTTONDBLCLK:

                    break;

                case WndProcMessageType.WM_MBUTTONDOWN:

                    break;

                case WndProcMessageType.WM_MBUTTONUP:

                    break;

                case WndProcMessageType.WM_MBUTTONDBLCLK:

                    break;

                case WndProcMessageType.WM_RBUTTONDOWN:

                    break;

                case WndProcMessageType.WM_RBUTTONUP:

                    break;

                case WndProcMessageType.WM_RBUTTONDBLCLK:

                    break;

                case WndProcMessageType.WM_XBUTTONDOWN:

                    break;

                case WndProcMessageType.WM_XBUTTONUP:

                    break;

                case WndProcMessageType.WM_XBUTTONDBLCLK:

                    break;
            }
        }

        /// <summary>
        /// Parses a message representing a mouse cursor action with coordinates attached to it.
        /// </summary>
        /// <param name="state">The <see cref="MouseButtonState"/> to update.</param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        private void ParseCursorAction(ref MouseButtonState state, long wParam, long lParam)
        {
            state.Position = new Vector2I(lParam);
        }

        private void MouseBufferSize_OnChanged(int oldValue, int newValue)
        {
            _mouse.Unacquire();
            _mouse.Properties.BufferSize = newValue;
            _mouse.Acquire();
        }

        protected override void OnBind(INativeSurface surface)
        {
            _surface = surface;
            SurfaceHandleChanged(surface);
            _surface.OnHandleChanged += SurfaceHandleChanged;
            _surface.OnParentChanged += SurfaceHandleChanged;
        }

        protected override void OnUnbind(INativeSurface surface)
        {
            _surface.OnHandleChanged -= SurfaceHandleChanged;
            _surface.OnParentChanged -= SurfaceHandleChanged;
            _surface = null;
        }

        private void SurfaceHandleChanged(INativeSurface surface)
        {
            if (surface.WindowHandle != null)
                _windowHandle = surface.WindowHandle.Value;
        }

        public override void OpenControlPanel()
        {
            _mouse.RunControlPanel();
        }

        /// <summary>Positions the mouse cursor at the center of the window.</summary>
        public void CenterInView()
        {
            Rectangle winBounds = _surface.Bounds;
            Vector2I p = winBounds.Center;

            _position = new Vector2I(p.X, p.Y);
        }

        private Vector2I ToLocalPosition(Vector2I pos)
        {
            if (_surface != null)
            {
                Rectangle oBounds = _surface.Bounds;
                pos -= new Vector2I(oBounds.X, oBounds.Y);
            }
            return pos;

        }

        private Vector2I ToDesktopPosition(Vector2I pos)
        {
            if (_surface != null)
            {
                Rectangle oBounds = _surface.Bounds;
                pos += new Vector2I(oBounds.X, oBounds.Y);
            }
            return pos;
        }

        protected override void OnClearState()
        {
            _wheelDelta = 0;
            _moved = Vector2I.Zero;
            _state = new MouseState();
            _prevState = new MouseState();
        }

        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of game time to use.</param>
        protected override void OnUpdate(Timing time)
        {
            if (_surface == null)
                return;

            IntPtr forewindow = Win32.GetForegroundWindow();
            Rectangle winBounds = _surface.Bounds;

            // Update previous state with previous buffer data
            if (_pollBuffer != null && _bufferUpdated)
            {
                for (int i = 0; i < _pollBuffer.Length; i++)
                    _prevState.Update(_pollBuffer[i]);
            }

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

                // Get latest info from mouse and buffer.
                _mouse.Poll();
                _pollBuffer = _mouse.GetBufferedData();
                _bufferUpdated = true;

                // If the mouse is in a valid window, process movement, position, etc
                if (insideControl || IsConstrained)
                {
                    // Send all buffered updates to mouse state
                    for (int i = 0; i < _pollBuffer.Length; i++)
                    {
                        _state.Update(_pollBuffer[i]);
                        _wheelDelta += _state.Z;
                    }

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

        protected override void OnDispose()
        {
            SetCursorVisiblity(true);
            DisposeObject(ref _mouse);
        }

        /// <summary>Returns the amount the mouse cursor has moved a long X and Y since the last frame/update.</summary>
        public Vector2I Delta => _moved;

        /// <summary>Gets the amount the mouse wheel has been moved since the last frame.</summary>
        public float WheelDelta => _wheelDelta;

        /// <summary>Gets the current scroll wheel position.</summary>
        public float WheelPosition => _wheelPos;

        /// <summary>Gets or sets the position of the mouse cursor.</summary>
        public override Vector2I Position
        {
            get => ToLocalPosition(_position);
            private set
            {
                _position = ToDesktopPosition(value);
                Cursor.Position = new System.Drawing.Point((int)_position.X, (int)_position.Y);
            }
        }

        /// <summary>Gets or sets whether or not the mouse cursor is visible.</summary>
        public override bool CursorVisible
        {
            get => _requestedVisibility;
            set => _requestedVisibility = value;
        }

        public override string DeviceName => _mouse.Information.ProductName;
    }
}

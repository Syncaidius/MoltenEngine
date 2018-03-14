using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectInput;
using SharpDX.Multimedia;
using SharpDX.RawInput;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Windows;
using Molten.Graphics;
using Molten.Utilities;
using System.Diagnostics;
using System.Windows.Forms;

namespace Molten.IO
{
    public delegate void MouseEventHandler(MouseDevice mouse);

    /// <summary>Handles mouse input.</summary>
    public class MouseDevice : InputHandlerBase<MouseButton>, IMouseDevice
    {
        /// <summary>
        /// Occurs when the mouse cursor was inside the parent window/control, but just left it.
        /// </summary>
        public event MouseEventHandler OnLeaveControl;

        /// <summary>
        /// Occurs when the mouse cursor was outside of the parent window/control, but just entered it.
        /// </summary>
        public event MouseEventHandler OnEnterControl;

        Mouse _mouse;
        MouseState _state;
        MouseState _prevState;

        Vector2F _position;
        Vector2F _prevPosition;
        Vector2F _moved;

        float _wheelPos;
        float _prevWheelPos;
        float _wheelDelta;

        MouseUpdate[] _buffer;

        bool _wasInsideControl = false;
        bool _requestedVisibility = true;
        bool _cursorVisibleState = true;
        IWindowSurface _surface;

        internal override void Initialize(IInputManager manager, Logger log, IWindowSurface surface)
        {
            InputManager diManager = manager as InputManager;

            _surface = surface;
            _mouse = new Mouse(diManager.DirectInput);
            _mouse.Properties.AxisMode = DeviceAxisMode.Relative;
            _mouse.Properties.BufferSize = 256;
            _mouse.Acquire();

            _state = new MouseState();
            _prevState = new MouseState();
        }

        public override void OpenControlPanel()
        {
            _mouse.RunControlPanel();
        }

        /// <summary>Positions the mouse cursor at the center of the window.</summary>
        public void CenterInWindow()
        {
            Rectangle winBounds = _surface.Bounds;
            Vector2I p = winBounds.Center;

            _position = new Vector2F(p.X, p.Y);
        }

        /// <summary>Returns true if the given buttonboard button is pressed.</summary>
        /// <param name="button">The button(s) to check.</param>
        /// <returns>True if pressed.</returns>
        public override bool IsPressed(MouseButton button)
        {
            int butval = (int)button;

            return _state.Buttons[butval];
        }

        /// <summary>Returns true if the button is pressed, but wasn't already pressed previously.</summary>
        /// <param name="button">THe button(s) to test against.</param>
        /// <returns>Returns true if the button is pressed, but wasn't already pressed previously.</returns>
        public override bool IsTapped(MouseButton button)
        {
            int butval = (int)button;
            return _state.Buttons[butval] && _prevState.Buttons[butval] == false;
        }

        /// <summary>Returns true if the specified button have been held for at least the given interval of time.</summary>
        /// <param name="button">The button(s) to do a held test for.</param>
        /// <param name="interval">The interval of time the button(s) must be held for to be considered as held.</param>
        /// <param name="reset">Set to true if the current amount of time the button has been held should be reset.</param>
        /// <returns>True if button(s) considered held.</returns>
        public override bool IsHeld(MouseButton button, int interval, bool reset)
        {
            int butval = (int)button;
            return _state.Buttons[butval] && _prevState.Buttons[butval];
        }

        private Vector2F ToLocalPosition(Vector2F pos)
        {
            Rectangle oBounds = _surface.Bounds;
            pos -= new Vector2F(oBounds.X, oBounds.Y);
            return pos;
        }

        private Vector2F ToDesktopPosition(Vector2F pos)
        {
            Rectangle oBounds = _surface.Bounds;
            pos += new Vector2F(oBounds.X, oBounds.Y);
            return pos;
        }

        public override void ClearState()
        {
            throw new NotImplementedException();
        }

        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of game time to use.</param>
        internal override void Update(Timing time)
        {
            // Update previous state with previous buffer data
            if (_buffer != null)
                for (int i = 0; i < _buffer.Length; i++)
                    _prevState.Update(_buffer[i]);

            _moved = new Vector2F();
            _wheelDelta = 0f;

            _state.X = 0;
            _state.Y = 0;
            _state.Z = 0;

            // Store previous position
            _prevPosition = _position;
            _prevWheelPos = _wheelPos;

            // Get latest info from mouse and buffer it
            _mouse.Poll();
            _buffer = _mouse.GetBufferedData();
            IntPtr forewindow = Win32.GetForegroundWindow();
            IntPtr outputHandle = _surface.WindowHandle;
            Rectangle winBounds = _surface.Bounds;

            // Make sure the game window is focused before updating movement/position.
            if (forewindow == outputHandle)
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

                EnterLeave(insideControl);

                // If the mouse is in a valid window, process movement, position, etc
                if (insideControl || IsConstrained)
                {                    
                    // Send all buffered updates to mouse state
                    for (int i = 0; i < _buffer.Length; i++)
                    {
                        _state.Update(_buffer[i]);
                        _moved.X += _state.X;
                        _moved.Y += _state.Y;
                        _wheelDelta += _state.Z;
                    }

                    _position += _moved;
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

                    // Apply cursor position.
                    Cursor.Position = new System.Drawing.Point((int)_position.X, (int)_position.Y);

                    // Perform correction if we exceeded Windows cursor limits.
                    if (_position.X != Cursor.Position.X)
                        _position.X = Cursor.Position.X;

                    if (_position.Y != Cursor.Position.Y)
                        _position.Y = Cursor.Position.Y;

                    // Update cursor visibility
                    SetCursorVisiblity(_requestedVisibility);
                }
                else
                {
                    _position = new Vector2F(winPos.X, winPos.Y);
                    _moved = new Vector2F();
                    SetCursorVisiblity(true);
                }
            }
            else
            {
                EnterLeave(false);
                _moved = new Vector2F();
                SetCursorVisiblity(true);
            }
        }

        private void EnterLeave(bool insideControl)
        {
            if (insideControl && !_wasInsideControl)
                OnEnterControl?.Invoke(this);
            else if (!insideControl && _wasInsideControl)
                OnLeaveControl?.Invoke(this);

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
        public Vector2F Moved => _moved;

        /// <summary>Gets the amount the mouse wheel has been moved since the last frame.</summary>
        public float WheelDelta => _wheelDelta;

        /// <summary>Gets the current scroll wheel position.</summary>
        public float WheelPosition => _wheelPos;

        /// <summary>Gets or sets the position of the mouse cursor.</summary>
        public Vector2F Position
        {
            get
            {
                return ToLocalPosition(_position);
            }
            set
            {
                _position = ToDesktopPosition(value);
                Cursor.Position = new System.Drawing.Point((int)_position.X, (int)_position.Y);
            }
        }

        /// <summary>Gets or sets whether or not the mouse cursor is visible.</summary>
        public bool CursorVisible
        {
            get { return _requestedVisibility; }
            set { _requestedVisibility = value; }
        }

        /// <summary>Gets or sets whether or not the mouse is contrained to the bounds of the main output.</summary>
        public bool IsConstrained { get; set; }

        public override string DeviceName => _mouse.Information.ProductName;

        /// <summary>Gets whether or not the mouse is attached.</summary>
        public override bool IsConnected => true;
    }
}

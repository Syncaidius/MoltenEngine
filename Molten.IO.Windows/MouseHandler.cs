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

namespace Molten.Input
{
    /// <summary>Handles mouse input.</summary>
    public class MouseHandler : InputHandlerBase<MouseButton>
    {
        Mouse _mouse;
        MouseState _state;
        MouseState _prevState;

        Vector2 _position;
        Vector2 _prevPosition;
        Vector2 _moved;

        float _wheelPos;
        float _prevWheelPos;
        float _wheelDelta;

        MouseUpdate[] _buffer;

        bool _inControl = false;
        bool _cursorVisible = true;
        bool _cursorVisibleState = true;
        IWindowSurface _surface;

        protected override void OnInitialize(InputManager manager, Logger log, IWindowSurface surface)
        {
            _surface = surface;
            _mouse = new Mouse(manager.DirectInput);
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
            IntVector2 p = winBounds.Center;

            _position = new Vector2(p.X, p.Y);
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

        private Vector2 ToLocalPosition(Vector2 pos)
        {
            Rectangle oBounds = _surface.Bounds;
            pos -= new Vector2(oBounds.X, oBounds.Y);

            return pos;
        }

        private Vector2 ToDesktopPosition(Vector2 pos)
        {
            Rectangle oBounds = _surface.Bounds;
            pos += new Vector2(oBounds.X, oBounds.Y);

            return pos;
        }

        public override void ClearState()
        {
            throw new NotImplementedException();
        }

        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of game time to use.</param>
        public override void Update(Timing time)
        {
            // Update previous state with previous buffer data
            if (_buffer != null)
                for (int i = 0; i < _buffer.Length; i++)
                    _prevState.Update(_buffer[i]);

            _moved = new Vector2();
            _wheelDelta = 0f;

            _state.X = 0;
            _state.Y = 0;
            _state.Z = 0;

            // Store previous position
            _prevPosition = _position;

            // Get latest info from mouse and buffer it
            _mouse.Poll();
            _buffer = _mouse.GetBufferedData();
             
            IntPtr forewindow = Win32.GetForegroundWindow();
            IntPtr outputHandle = _surface.WindowHandle;
            Rectangle winBounds = _surface.Bounds;
            //Rectangle displayBounds = _surface.DisplayBounds;

            // Make sure the game window is focused before updating movement/position.
            if (forewindow == outputHandle)
            {
                // The windows cursor position
                System.Drawing.Point winPos = System.Windows.Forms.Cursor.Position;

                // Check if the cursor has gone outside of the control/window 
                _inControl = true;
                if (winPos.X < winBounds.Left)
                    _inControl = false;
                else if (winPos.Y < winBounds.Top)
                    _inControl = false;
                else if (winPos.X > winBounds.Right)
                    _inControl = false;
                else if (winPos.Y > winBounds.Bottom)
                    _inControl = false;

                // If the mouse is in a valid window, process movement, position, etc
                if (_inControl || IsConstrained)
                {                    
                    //send all buffered updates to mouse state
                    for (int i = 0; i < _buffer.Length; i++)
                    {
                        _state.Update(_buffer[i]);
                        _moved.X += _state.X;
                        _moved.Y += _state.Y;
                        _wheelDelta += _state.Z;
                    }

                    _position += _moved;

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
                    //else
                    //{
                    //    if (_position.X < displayBounds.X)
                    //        _position.X = displayBounds.X;
                    //    if (_position.Y < displayBounds.Y)
                    //        _position.Y = displayBounds.Y;
                    //    if (_position.X > displayBounds.Width)
                    //        _position.X = displayBounds.Width;
                    //    if (_position.Y > displayBounds.Height)
                    //        _position.Y = displayBounds.Height;
                    //}

                    //adjust position based on sensitivity
                    Cursor.Position = new System.Drawing.Point((int)_position.X, (int)_position.Y);

                    //Update cursor visibility
                    ToggleCursorVisiblity(_cursorVisible);
                }
                else
                {
                    _position = new Vector2(winPos.X, winPos.Y);
                    _moved = new Vector2();
                    ToggleCursorVisiblity(true);
                }
            }
            else
            {
                _moved = new Vector2();
                ToggleCursorVisiblity(true);
            }
        }

        /// <summary>Safely handles the cursor's visibility state, since calls to show and hide are counted. 
        /// Two calls to .Show and a last call to .Hide will not hide the cursor.</summary>
        /// <param name="visible">If true, the cursor will be made visible, if not already.</param>
        private void ToggleCursorVisiblity(bool visible)
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
            ToggleCursorVisiblity(true);

            DisposeObject(ref _mouse);
        }

        /// <summary>Returns the amount the mouse cursor has moved a long X and Y since the last frame/update.</summary>
        public Vector2 Moved
        {
            get { return _moved; }
        }

        /// <summary>Gets the amount the mouse wheel has been moved since the last frame.</summary>
        public float WheelDelta
        {
            get { return _wheelDelta; }
        }

        /// <summary>Gets the current scroll wheel position.</summary>
        public float WheelPosition
        {
            get { return _wheelPos; }
        }

        /// <summary>Gets whether or not the mouse is inside the application control or window.</summary>
        public bool InsideControl
        {
            get { return _inControl; }
        }

        /// <summary>Gets or sets the position of the mouse cursor.</summary>
        public Vector2 Position
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
            get { return _cursorVisible; }
            set { _cursorVisible = value; }
        }

        /// <summary>Gets or sets whether or not the mouse is contrained to the bounds of the main output.</summary>
        public bool IsConstrained { get; set; }

        public override string DeviceName
        {
            get { return _mouse.Information.ProductName; }
        }

        /// <summary>Gets whether or not the mouse is attached.</summary>
        public override bool IsConnected
        {
            get { return true; }
        }
    }

    public enum MouseButton : byte
    {
        Left = 0,
        Right = 1,
        Middle = 2,
        XButton1 = 3,
        XButton2 = 4,

        None = 255,
    }
}

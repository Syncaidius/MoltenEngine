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
    public class MouseDevice : InputHandlerBase<MouseButton>, IMouseDevice
    {
        /// <summary>
        /// Occurs when the mouse cursor was inside the parent window/control, but just left it.
        /// </summary>
        public event MouseEventHandler OnLeaveSurface;

        /// <summary>
        /// Occurs when the mouse cursor was outside of the parent window/control, but just entered it.
        /// </summary>
        public event MouseEventHandler OnEnterSurface;

        Mouse _mouse;
        MouseState _state;
        MouseState _prevState;

        Vector2I _position;
        Vector2I _prevPosition;
        Vector2I _moved;

        float _wheelPos;
        float _prevWheelPos;
        float _wheelDelta;

        MouseUpdate[] _buffer;

        bool _wasInsideControl = false;
        bool _requestedVisibility = true;
        bool _cursorVisibleState = true;
        IWindowSurface _surface;
        IntPtr _windowHandle;
        bool _bufferUpdated;

        internal override void Initialize(IInputManager manager, Logger log)
        {
            InputManager diManager = manager as InputManager;

            _mouse = new Mouse(diManager.DirectInput);
            _mouse.Properties.AxisMode = DeviceAxisMode.Relative;
            _mouse.Properties.BufferSize = 256;
            _mouse.Acquire();

            _state = new MouseState();
            _prevState = new MouseState();
        }

        internal override void Bind(IWindowSurface surface)
        {
            _surface = surface;
            SurfaceHandleChanged(surface);
            _surface.OnHandleChanged += SurfaceHandleChanged;
        }

        internal override void Unbind(IWindowSurface surface)
        {
            _surface = null;
        }

        private void SurfaceHandleChanged(IWindowSurface surface)
        {
            IntPtr? handle = GetWindowHandle(surface);

            if (handle != null)
                _windowHandle = handle.Value;
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

        /// <summary>Returns true if the specified button was pressed in both the previous and current frame.</summary>
        /// <param name="button">The button(s) to do a held test for.</param>
        /// <returns>True if button(s) considered held.</returns>
        public override bool IsHeld(MouseButton button)
        {
            int butval = (int)button;
            return _state.Buttons[butval] && _prevState.Buttons[butval];
        }

        private Vector2I ToLocalPosition(Vector2I pos)
        {
            Rectangle oBounds = _surface.Bounds;
            pos -= new Vector2I(oBounds.X, oBounds.Y);
            return pos;
        }

        private Vector2I ToDesktopPosition(Vector2I pos)
        {
            Rectangle oBounds = _surface.Bounds;
            pos += new Vector2I(oBounds.X, oBounds.Y);
            return pos;
        }

        public override void ClearState()
        {
            _wheelDelta = 0;
            _moved = Vector2I.Zero;
            _state = new MouseState();
            _prevState = new MouseState();
        }

        /// <summary>Update input handler.</summary>
        /// <param name="time">The snapshot of game time to use.</param>
        internal override void Update(Timing time)
        {
            IntPtr forewindow = Win32.GetForegroundWindow();
            Rectangle winBounds = _surface.Bounds;

            // Update previous state with previous buffer data
            if (_buffer != null && _bufferUpdated)
            {
                for (int i = 0; i < _buffer.Length; i++)
                    _prevState.Update(_buffer[i]);
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
                _buffer = _mouse.GetBufferedData();
                _bufferUpdated = true;

                // If the mouse is in a valid window, process movement, position, etc
                if (insideControl || IsConstrained)
                {
                    // Send all buffered updates to mouse state
                    for (int i = 0; i < _buffer.Length; i++)
                    {
                        _state.Update(_buffer[i]);
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
        public Vector2I Position
        {
            get => ToLocalPosition(_position);
            set
            {
                _position = ToDesktopPosition(value);
                Cursor.Position = new System.Drawing.Point((int)_position.X, (int)_position.Y);
            }
        }

        /// <summary>Gets or sets whether or not the mouse cursor is visible.</summary>
        public bool CursorVisible
        {
            get => _requestedVisibility;
            set => _requestedVisibility = value;
        }

        /// <summary>Gets or sets whether or not the mouse is contrained to the bounds of the main output.</summary>
        public bool IsConstrained { get; set; }

        public override string DeviceName => _mouse.Information.ProductName;

        /// <summary>Gets whether or not the mouse is attached.</summary>
        public override bool IsConnected => true;
    }
}

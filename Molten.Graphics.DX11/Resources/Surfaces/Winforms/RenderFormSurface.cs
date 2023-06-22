namespace Molten.Graphics.DX11
{
    /// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
    public class RenderFormSurface : WinformsSurface<RenderForm>, INativeSurface
    {
        System.Drawing.Size? _preBorderlessSize;
        System.Drawing.Point? _preBorderlessLocation;
        System.Drawing.Rectangle? _preBorderlessScreenArea;

        public event WindowSurfaceHandler OnClose;

        public event WindowSurfaceHandler OnMinimize;

        public event WindowSurfaceHandler OnRestore;

        public event WindowSurfaceHandler OnHandleChanged;

        public event WindowSurfaceHandler OnParentChanged;

        public event WindowSurfaceHandler OnFocusGained;

        public event WindowSurfaceHandler OnFocusLost;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formTitle"></param>
        /// <param name="formName">The internal name of the form.</param>
        /// <param name="renderer"></param>
        /// <param name="mipCount"></param>
        /// <param name="sampleCount"></param>
        internal RenderFormSurface(GraphicsDevice device, string formTitle, string formName, uint mipCount = 1)
            : base(device, formTitle, formName, mipCount) { }

        protected override void CreateControl(string title, out RenderForm control, out IntPtr handle)
        {
            control = new RenderForm(title);
            control.WindowState = FormWindowState.Maximized;
            handle = control.Handle;
            OnHandleChanged?.Invoke(this);
        }

        public void Close()
        {
            Control?.Close();
        }

        protected override void SubscribeToControl(RenderForm control)
        {
            // Subscribe to all the needed form events
            control.AllowUserResizing = true;
            control.UserResized += Control_Resized;
            control.Move += Control_Moved;
            control.FormClosing += Control_FormClosing;
            control.GotFocus += Control_GotFocus;
            control.LostFocus += Control_LostFocus;
        }

        private void Control_LostFocus(object sender, EventArgs e)
        {
            IsFocused = false;
            OnFocusLost?.Invoke(this);
        }

        private void Control_GotFocus(object sender, EventArgs e)
        {
            IsFocused = true;
            OnFocusGained?.Invoke(this);
        }

        private void Control_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            OnClose?.Invoke(this);
        }

        void Control_Moved(object sender, EventArgs e)
        {
            UpdateControlMode(Control, _mode);
        }

        void Control_Resized(object sender, EventArgs e)
        {
            uint w, h;

            if (Mode == WindowMode.Borderless)
            {
                w = (uint)Control.Bounds.Width;
                h = (uint)Control.Bounds.Height;
            }
            else
            {
                w = (uint)Control.ClientSize.Width;
                h = (uint)Control.ClientSize.Height;
            }

            if (w != Width || h != Height)
                Resize(GraphicsPriority.Apply, w, h);
        }

        protected override void UpdateControlMode(RenderForm control, WindowMode newMode)
        {
            if (_mode != newMode)
                Device.Log.WriteLine($"Form surface '{Name}' mode set to '{newMode}'");

            // Update current mode
            _mode = newMode;
            System.Drawing.Rectangle clientArea = Control.ClientRectangle;

            // Handle new mode
            switch (_mode)
            {
                case WindowMode.Windowed:
                    Control.FormBorderStyle = FormBorderStyle.Sizable;

                    // Calculate offset due to borders and title bars, based on the current mode of the window.
                    if (_preBorderlessLocation != null && _preBorderlessSize != null)
                    {
                        Control.Move -= Control_Moved;
                        Control.Location = _preBorderlessLocation.Value;
                        Control.Size = _preBorderlessSize.Value;
                        System.Drawing.Rectangle screenArea = _preBorderlessScreenArea.Value;
                        Control.Move += Control_Moved;

                        _bounds = new Rectangle()
                        {
                            X = screenArea.X,
                            Y = screenArea.Y,
                            Width = screenArea.Width,
                            Height = screenArea.Height,
                        };

                        // Clear pre-borderless dimensions.
                        _preBorderlessLocation = null;
                        _preBorderlessSize = null;
                        _preBorderlessScreenArea = null;
                    }
                    else
                    {

                        System.Drawing.Rectangle screenArea = Control.RectangleToScreen(clientArea);
                        _bounds = new Rectangle()
                        {
                            X = screenArea.X,
                            Y = screenArea.Y,
                            Width = screenArea.Width,
                            Height = screenArea.Height,
                        };
                    }

                    break;

                case WindowMode.Borderless:
                    // Store pre-borderless form dimensions.
                    if (_preBorderlessLocation == null && _preBorderlessSize == null)
                    {
                        _preBorderlessLocation = Control.Location;
                        _preBorderlessSize = Control.Size;
                        _preBorderlessScreenArea = Control.RectangleToScreen(clientArea);
                    }

                    System.Drawing.Rectangle dBounds = Screen.GetBounds(Control);

                    Control.WindowState = FormWindowState.Normal;
                    Control.FormBorderStyle = FormBorderStyle.None;
                    Control.TopMost = false;

                    Control.Bounds = dBounds;
                    _bounds = new Rectangle()
                    {
                        X = dBounds.X,
                        Y = dBounds.Y,
                        Width = dBounds.Width,
                        Height = dBounds.Height,
                    };
                    break;
            }
        }

        protected override void DisposeControl()
        {
            if (Parent != null)
                Parent.Move -= Control_Moved;

            Control.UserResized -= Control_Resized;
            Control.Move -= Control_Moved;
            Control.FormClosing -= Control_FormClosing;
            Control.GotFocus -= Control_GotFocus;
            Control.LostFocus -= Control_LostFocus;

            ParentHandle = null;
            Control?.Dispose();
        }

        protected override void OnNewParent(Control newParent, RenderForm control)
        {
            if(newParent is Form parentForm)
            {
                control.MdiParent = parentForm;
                OnParentChanged?.Invoke(this);
            }
            else
            {
                throw new InvalidOperationException("RenderFormSurface cannot be parented to a non-form or non-window control.");
            }
        }

        public nint? WindowHandle => ControlHandle;
    }
}

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Molten.Graphics
{
    /// <summary>A render target that is created from, and outputs to, a GUI control-based swap chain.</summary>
    public class RenderControlSurface : WinformsSurface<RenderControl>, INativeSurface
    {
        public event WindowSurfaceHandler OnClose;

        public event WindowSurfaceHandler OnMinimize;

        public event WindowSurfaceHandler OnRestore;

        public event WindowSurfaceHandler OnHandleChanged;

        public event WindowSurfaceHandler OnParentChanged;

        public event WindowSurfaceHandler OnFocusGained;

        public event WindowSurfaceHandler OnFocusLost;

        IntPtr? _windowHandle;
        Control _parentWindow;

        internal RenderControlSurface(string controlTitle, string controlName, RendererDX11 renderer, int mipCount = 1, int sampleCount = 1)
            : base(controlTitle, controlName, renderer, mipCount, sampleCount) { }

        protected override void CreateControl(string title, ref RenderControl control, ref IntPtr handle)
        {
            control = new RenderControl()
            {
                Size = new System.Drawing.Size(1, 1),
            };
            handle = control.Handle;
            OnHandleChanged?.Invoke(this);
        }

        protected override void SubscribeToControl(RenderControl control)
        {
            // Subscribe to all the needed form events
            GetParentWindow();
            control.Resize += _control_Resized;
            control.Move += _control_Moved;
            control.ParentChanged += _control_ParentChanged;
            control.HandleDestroyed += _control_HandleDestroyed;
            control.VisibleChanged += _control_VisibleChanged;
            control.GotFocus += _control_GotFocus;
            control.LostFocus += _control_LostFocus;
        }

        private void _control_LostFocus(object sender, EventArgs e)
        {
            IsFocused = false;
            OnFocusGained?.Invoke(this);
        }

        private void _control_GotFocus(object sender, EventArgs e)
        {
            IsFocused = false;
            OnFocusLost?.Invoke(this);
        }

        private void _control_ParentChanged(object sender, EventArgs e)
        {
            // Unsubscribe from old parent
            if(_parentWindow != null)
                _parentWindow.Move -= _control_Moved;

            GetParentWindow();
            UpdateControlMode(Control, _mode);
        }

        private void GetParentWindow()
        {
            _windowHandle = null;
            _parentWindow = null;

            // Check if the surface handle is a form. 
            // If not, find it's parent form.
            Control ctrl = Control;
            while (ctrl != null)
            {
                if (ctrl is Form frm)
                {
                    _windowHandle = frm.Handle;
                    _parentWindow = frm;
                    break;
                }
                else
                {
                    ctrl = ctrl.Parent;
                }
            }

            // Subscribe to new parent
            if (_parentWindow != null)
                _parentWindow.Move += _control_Moved;

            _control_Resized(_parentWindow, new EventArgs());
        }

        private void _control_VisibleChanged(object sender, EventArgs e)
        {
            Visible = Control.Visible;
        }

        private void _control_HandleDestroyed(object sender, EventArgs e)
        {
            OnClose?.Invoke(this);
        }

        void _control_Moved(object sender, EventArgs e)
        {
            UpdateControlMode(Control, _mode);
        }

        void _control_Resized(object sender, EventArgs e)
        {
            int w = Control.ClientSize.Width;
            int h = Control.ClientSize.Height;

            if (w != _width || h != _height)
                Resize(w, h);
        }

        protected override void UpdateControlMode(RenderControl control, WindowMode mode)
        {
            // Calculate offset due to borders and title bars, based on the current mode of the window.
            System.Drawing.Rectangle clientArea = control.ClientRectangle;
            System.Drawing.Rectangle screenArea = control.RectangleToScreen(clientArea);

            _bounds = new Rectangle()
            {
                X = screenArea.X,
                Y = screenArea.Y,
                Width = screenArea.Width,
                Height = screenArea.Height,
            };
        }

        protected override void DisposeControl()
        {
            if (_parentWindow != null)
                _parentWindow.Move -= _control_Moved;

            Control.Resize -= _control_Resized;
            Control.Move -= _control_Moved;
            Control.ParentChanged -= _control_ParentChanged;
            Control.HandleDestroyed -= _control_HandleDestroyed;
            Control.VisibleChanged -= _control_VisibleChanged;
            Control.GotFocus -= _control_GotFocus;
            Control.LostFocus -= _control_LostFocus;

            ParentHandle = null;
            _parentWindow = null;
            _windowHandle = null;
            Control.Dispose();
        }

        protected override void OnNewParent(Control newParent, RenderControl control)
        {
            control.Size = newParent.Size;
            control.Location = newParent.Location;
            control.Dock = DockStyle.Fill;
            OnParentChanged?.Invoke(this);
        }

        public IntPtr? WindowHandle => _windowHandle;
    }
}

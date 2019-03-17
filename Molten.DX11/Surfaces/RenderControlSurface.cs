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

        public event WindowSurfaceHandler OnFocusGained;

        public event WindowSurfaceHandler OnFocusLost;

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
            DetectRootParent();
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
            OnFocusGained?.Invoke(this);
        }

        private void _control_GotFocus(object sender, EventArgs e)
        {
            OnFocusLost?.Invoke(this);
        }

        private void _control_ParentChanged(object sender, EventArgs e)
        {
            // Unsubscribe from old parent
            if(Parent != null)
            {
                Parent.Move -= _control_Moved;
            }

            DetectRootParent();
            UpdateControlMode(Control, _mode);
        }

        private void DetectRootParent()
        {
            Control candidate = Control;
            while(candidate.Parent != null)
                candidate = candidate.Parent;

            Parent = candidate;

            // Subscribe to new parent
            if (Parent != null)
                Parent.Move += _control_Moved;

            _control_Resized(Parent, new EventArgs());
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
            if (Parent != null)
                Parent.Move -= _control_Moved;

            Control.Resize -= _control_Resized;
            Control.Move -= _control_Moved;
            Control.ParentChanged -= _control_ParentChanged;
            Control.HandleDestroyed -= _control_HandleDestroyed;
            Control.VisibleChanged -= _control_VisibleChanged;
            Control.GotFocus -= _control_GotFocus;
            Control.LostFocus -= _control_LostFocus;

            Parent = null;
            Control.Dispose();
        }
    }
}

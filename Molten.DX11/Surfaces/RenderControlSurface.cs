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
    public class RenderControlSurface : SwapChainSurface, IWindowSurface
    {
        RenderLoop _loop;
        RenderControl _control;
        Control _parent;
        Rectangle _bounds;
        IntPtr _handle;
        DisplayMode _displayMode;
        string _title;
        bool _disposing;

        public event WindowSurfaceHandler OnClose;

        public event WindowSurfaceHandler OnMinimize;

        public event WindowSurfaceHandler OnRestore;

        public event WindowSurfaceHandler OnHandleChanged;

        internal RenderControlSurface(string formTitle, RendererDX11 renderer, int mipCount = 1, int sampleCount = 1)
            : base(renderer, mipCount, sampleCount)
        {
            _title = formTitle;
        }

        protected override void OnSwapChainMissing()
        {
            _control = new RenderControl()
            {
                Size = new System.Drawing.Size(1,1),
            };
            _handle = _control.Handle;
            OnHandleChanged?.Invoke(this);

            _loop = new RenderLoop(_control)
            {
                UseApplicationDoEvents = false,
            };

            //set default bounds
            UpdateControlMode();

            ModeDescription modeDesc = new ModeDescription()
            {
                Width = _bounds.Width,
                Height = _bounds.Height,
                RefreshRate = new Rational(60, 1),
                Format = DxFormat,
                Scaling = DisplayModeScaling.Stretched,
                ScanlineOrdering = DisplayModeScanlineOrder.Progressive,
            };

            _displayMode = new DisplayMode(modeDesc);
            CreateSwapChain(_displayMode, true, _control.Handle);

            // Subscribe to all the needed form events
            DetectRootParent();
            _control.Resize += _control_Resized;
            _control.Move += _control_Moved;
            _control.ParentChanged += _control_ParentChanged;
            _control.HandleDestroyed += _control_HandleDestroyed;
            _control.VisibleChanged += _control_VisibleChanged;

            _control.KeyDown += (sender, args) =>
            {
                if (args.Alt)
                    args.Handled = true;
            };

            // Ignore all windows events
            Device.DisplayManager.DxgiFactory.MakeWindowAssociation(_control.Handle, WindowAssociationFlags.IgnoreAltEnter);
        }

        private void _control_ParentChanged(object sender, EventArgs e)
        {
            // Unsubscribe from old parent
            if(_parent != null)
            {
                _parent.Move -= _control_Moved;
            }

            DetectRootParent();
            UpdateControlMode();
        }

        private void DetectRootParent()
        {
            Control candidate = _control;
            while(candidate.Parent != null)
                candidate = candidate.Parent;

            _parent = candidate;

            // Subscribe to new parent
            if (_parent != null)
                _parent.Move += _control_Moved;
        }

        private void _control_VisibleChanged(object sender, EventArgs e)
        {
            Visible = _control.Visible;
        }

        private void _control_HandleDestroyed(object sender, EventArgs e)
        {
            OnClose?.Invoke(this);
        }

        void _control_Moved(object sender, EventArgs e)
        {
            UpdateControlMode();
        }

        void _control_Resized(object sender, EventArgs e)
        {
            int w = _control.ClientSize.Width;
            int h = _control.ClientSize.Height;

            if (w != _width || h != _height)
                Resize(w, h);
        }

        private void UpdateControlMode()
        {
            // Calculate offset due to borders and title bars, based on the current mode of the window.
            System.Drawing.Rectangle clientArea = _control.ClientRectangle;
            System.Drawing.Rectangle screenArea = _control.RectangleToScreen(clientArea);

            _bounds = new Rectangle()
            {
                X = screenArea.X,
                Y = screenArea.Y,
                Width = screenArea.Width,
                Height = screenArea.Height,
            };
        }

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
        {
            if (_displayMode.Width != newWidth || _displayMode.Height != newHeight)
            {

                _displayMode.Width = newWidth;
                _displayMode.Height = newHeight;

                // TODO validate display mode here. If invalid or unsupported by display, choose nearest supported.

                UpdateControlMode();
                _swapChain.ResizeTarget(ref _displayMode.Description);
                Device.Log.WriteLine($"Form surface '{_title}' resized to {newWidth}x{newHeight}");
            }
            else
            {
                UpdateControlMode();
            }

            base.UpdateDescription(newWidth, newHeight, newDepth, newMipMapCount, newArraySize, newFormat);
        }

        protected override bool OnPresent()
        {
            if (_disposing)
            {
                DisposeObject(ref _loop);
                DisposeObject(ref _swapChain);
                DisposeObject(ref _control);
                return false;
            }

            if (_loop.NextFrame())
            {
                if (Visible != _control.Visible)
                {
                    if (Visible)
                    {
                        _control.Show();
                    }
                    else
                    {
                        _control.Hide();
                        return false;
                    }
                }
            }

            return true;
        }

        private protected override void OnPipelineDispose()
        {
            if (_swapChain != null)
                _disposing = true;

            base.OnPipelineDispose();
        }

        /// <summary>Gets or sets the form title.</summary>
        public string Title
        {
            get => _control.Text;
            set => _control.Text = value;
        }

        public IntPtr Handle => _handle;

        /// <summary>Gets or sets the WinForms cursor for the controller.</summary>
        public Cursor Cursor
        {
            get => _control.Cursor;
            set => _control.Cursor = value;
        }

        /// <summary>Gets the bounds of the window surface.</summary>
        public Rectangle Bounds => _bounds;

        /// <summary>
        /// Gets or sets whether or not the control is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the window mode of the control.
        /// </summary>
        public WindowMode Mode { get; set; }

        #region Cast operators
        public static explicit operator UserControl(RenderControlSurface surface)
        {
            return surface._control;
        }
        #endregion
    }
}

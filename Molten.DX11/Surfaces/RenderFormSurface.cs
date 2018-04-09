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
    /// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
    public class RenderFormSurface : SwapChainSurface, IWindowSurface
    {
        RenderLoop _loop;
        RenderForm _form;
        Rectangle _bounds;
        IntPtr _formHandle;
        DisplayMode _displayMode;
        string _title;
        bool _visible;
        bool _disposing;

        WindowMode _mode = WindowMode.Windowed;
        WindowMode _requestedMode = WindowMode.Windowed;

        public event FormSurfaceHandler OnClose;

        public event FormSurfaceHandler OnMinimize;

        public event FormSurfaceHandler OnRestore;

        //public event FormSurfaceHandler OnResize;

        internal RenderFormSurface(string formTitle, GraphicsDevice device, int mipCount = 1, int sampleCount = 1)
            : base(device, mipCount, sampleCount)
        {
            _title = formTitle;
        }

        internal void MoveToOutput(DisplayOutput<Adapter1, AdapterDescription1, Output1> output)
        {
            MoveToOutput(output, _mode);
        }

        internal void MoveToOutput(DisplayOutput<Adapter1, AdapterDescription1, Output1> output, WindowMode mode)
        {
            // TODO move the surface's render form to the specified output
            // TODO resize the window to fit if it's too big.
            //      OR
            // TODO resize the window to fill the screen if fill = true;
            // TODO set ownership of window to whatever output it is moved to.
        }

        protected override SharpDX.Direct3D11.Resource CreateTextureInternal(bool resize)
        {
            // Resize the swap chain if needed.
            if (resize)
            {
                _swapChain.ResizeBuffers(_swapDesc.BufferCount, _width, _height, GraphicsFormat.Unknown.ToApi(), SwapChainFlags.None);
                _swapDesc = _swapChain.Description;
            }
            else
            {
                CreateFormAndSwapChain();
            }

            // Create new backbuffer from swap chain.
            _texture = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);
            _resource = _texture;
            _description = _texture.Description;
            _width = _description.Width;
            _height = _description.Height;
            RTV = new RenderTargetView(Device.D3d, _texture);
            VP = new Viewport(0, 0, _width, _height);

            if (!resize)
                AfterResize();

            return _texture;
        }

        private void CreateFormAndSwapChain()
        {
            Format format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            _form = new RenderForm(_title);
            _form.WindowState = FormWindowState.Maximized;
            _formHandle = _form.Handle;

            _loop = new RenderLoop(_form)
            {
                UseApplicationDoEvents = false,
            };

            SetVsync(Device.Settings.VSync);
            Device.Settings.VSync.OnChanged += VSync_OnChanged;

            //set default bounds
            UpdateFormMode(_requestedMode);

            ModeDescription modeDesc = new ModeDescription()
            {
                Width = _bounds.Width,
                Height = _bounds.Height,
                RefreshRate = new Rational(60, 1),
                Format = format,
                Scaling = DisplayModeScaling.Stretched,
                ScanlineOrdering = DisplayModeScanlineOrder.Progressive,
            };

            _displayMode = new DisplayMode(modeDesc);
            CreateSwapChain(_displayMode, true, _form.Handle);

            // Subscribe to all the needed form events
            _form.UserResized += _form_Resized;
            _form.Move += _form_Moved;
            _form.FormClosing += _form_FormClosing;

            _form.KeyDown += (sender, args) =>
            {
                if (args.Alt)
                    args.Handled = true;
            };

            // Ignore all windows events
            Device.DisplayManager.DxgiFactory.MakeWindowAssociation(_form.Handle, WindowAssociationFlags.IgnoreAltEnter);
        }

        private void _form_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            OnClose?.Invoke(this);
        }

        void _form_Moved(object sender, EventArgs e)
        {
            UpdateFormMode(_mode);
        }

        void _form_Resized(object sender, EventArgs e)
        {
            int w, h;

            if (_mode == WindowMode.Borderless)
            {
                w = _form.Bounds.Width;
                h = _form.Bounds.Height;
            }
            else
            {
                w = _form.ClientSize.Width;
                h = _form.ClientSize.Height;
            }

            if (w != _width || h != _height)
                Resize(w, h);
        }

        private void VSync_OnChanged(bool oldValue, bool newValue)
        {
            SetVsync(newValue);
        }

        private void UpdateFormMode(WindowMode newMode)
        {
            if (_mode != newMode)
                Device.Log.WriteLine($"Form surface '{_title}' mode set to '{newMode}'");

            // Update current mode
            _mode = newMode;

            // Handle new mode
            switch (_mode)
            {
                case WindowMode.Windowed:
                    _form.WindowState = FormWindowState.Maximized;
                    _form.FormBorderStyle = FormBorderStyle.FixedSingle;

                    // Calculate offset due to borders and title bars, based on the current mode of the window.
                    System.Drawing.Rectangle clientArea = _form.ClientRectangle;
                    System.Drawing.Rectangle screenArea = _form.RectangleToScreen(clientArea);

                    _bounds = new Rectangle()
                    {
                        X = screenArea.X,
                        Y = screenArea.Y,
                        Width = screenArea.Width,
                        Height = screenArea.Height,
                    }; ;

                    break;

                case WindowMode.Borderless:
                    System.Drawing.Rectangle dBounds = Screen.GetBounds(_form);

                    _form.WindowState = FormWindowState.Normal;
                    _form.FormBorderStyle = FormBorderStyle.None;
                    _form.TopMost = true;

                    _form.Bounds = dBounds;
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

        protected override void BeforeResize()
        {
            base.BeforeResize();
        }

        protected override void AfterResize()
        {
            base.AfterResize();
        }

        protected override void OnSetSize(int newWidth, int newHeight, int newDepth, int newArraySize)
        {
            if (_displayMode.Width != newWidth || _displayMode.Height != newHeight)
            {

                _displayMode.Width = newWidth;
                _displayMode.Height = newHeight;

                // TODO validate display mode here. If invalid or unsupported by display, choose nearest supported.

                UpdateFormMode(_mode);
                _swapChain.ResizeTarget(ref _displayMode.Description);
                Device.Log.WriteLine($"Form surface '{_title}' resized to {newWidth}x{newHeight}");
            }
            else
            {
                UpdateFormMode(_mode);
            }

            base.OnSetSize(newWidth, newHeight, newDepth, newArraySize);
        }

        public void Show()
        {
            _visible = true;
        }

        public void Hide()
        {
            _visible = false;
        }

        protected override bool OnPresent()
        {
            if (_disposing)
            {
                DisposeObject(ref _loop);
                DisposeObject(ref _swapChain);
                DisposeObject(ref _form);
                return false;
            }

            if(_mode != _requestedMode)
                UpdateFormMode(_requestedMode);

            if (_loop.NextFrame())
            {
                if (_visible != _form.Visible)
                {
                    if (_visible)
                    {
                        _form.Show();
                    }
                    else
                    {
                        _form.Hide();
                        return false;
                    }
                }

            }

            return true;
        }

        protected override void OnDisposeForRecreation()
        {
            // Avoid calling RenderFormSurface's OnDispose implementation by skipping it. Jump straight to base.
            base.OnDispose();
        }

        protected override void OnDispose()
        {
            if(_swapChain != null)
                _disposing = true;

            base.OnDispose();
        }

        /// <summary>Gets or sets the form title.</summary>
        public string Title
        {
            get => _form.Text;
            set => _form.Text = value;
        }

        public IntPtr WindowHandle => _formHandle;

        /// <summary>Gets or sets the WinForms cursor for the controller.</summary>
        public Cursor Cursor
        {
            get => _form.Cursor;
            set => _form.Cursor = value;
        }

        /// <summary>Gets or sets the mode of the output form.</summary>
        public WindowMode Mode
        {
            get => _requestedMode;
            set => _requestedMode = value;
        }

        /// <summary>Gets the bounds of the window surface.</summary>
        public Rectangle Bounds => _bounds;
    }
}

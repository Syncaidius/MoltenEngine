using Molten.Collections;
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
        bool _disposing;

        System.Drawing.Size? _preBorderlessSize;
        System.Drawing.Point? _preBorderlessLocation;
        System.Drawing.Rectangle? _preBorderlessScreenArea;

        WindowMode _mode = WindowMode.Windowed;
        WindowMode _requestedMode = WindowMode.Windowed;

        public event WindowSurfaceHandler OnClose;

        public event WindowSurfaceHandler OnMinimize;

        public event WindowSurfaceHandler OnRestore;

        public event WindowSurfaceHandler OnHandleChanged;

        internal RenderFormSurface(string formTitle, RendererDX11 renderer, int mipCount = 1, int sampleCount = 1)
            : base(renderer, mipCount, sampleCount)
        {
            _title = formTitle;
        }

        internal void MoveToOutput(DisplayOutputDX<Adapter1, AdapterDescription1, Output1> output)
        {
            MoveToOutput(output, _mode);
        }

        internal void MoveToOutput(DisplayOutputDX<Adapter1, AdapterDescription1, Output1> output, WindowMode mode)
        {
            // TODO move the surface's render form to the specified output
            // TODO resize the window to fit if it's too big.
            //      OR
            // TODO resize the window to fill the screen if fill = true;
            // TODO set ownership of window to whatever output it is moved to.
        }

        protected override void OnSwapChainMissing()
        {
            _form = new RenderForm(_title);
            _form.WindowState = FormWindowState.Maximized;
            _formHandle = _form.Handle;
            OnHandleChanged?.Invoke(this);

            _loop = new RenderLoop(_form)
            {
                UseApplicationDoEvents = false,
            };

            //set default bounds
            UpdateFormMode(_requestedMode);

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
            CreateSwapChain(_displayMode, true, _form.Handle);

            // Subscribe to all the needed form events
            _form.AllowUserResizing = true;
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

        private void UpdateFormMode(WindowMode newMode)
        {
            if (_mode != newMode)
                Device.Log.WriteLine($"Form surface '{_title}' mode set to '{newMode}'");

            // Update current mode
            _mode = newMode;
            System.Drawing.Rectangle clientArea = _form.ClientRectangle;

            // Handle new mode
            switch (_mode)
            {
                case WindowMode.Windowed:
                    _form.FormBorderStyle = FormBorderStyle.Sizable;

                    // Calculate offset due to borders and title bars, based on the current mode of the window.
                    if (_preBorderlessLocation != null && _preBorderlessSize != null)
                    {
                        _form.Move -= _form_Moved;
                        _form.Location = _preBorderlessLocation.Value;
                        _form.Size = _preBorderlessSize.Value;
                        System.Drawing.Rectangle screenArea = _preBorderlessScreenArea.Value;
                        _form.Move += _form_Moved;

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

                        System.Drawing.Rectangle screenArea = _form.RectangleToScreen(clientArea);
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
                        _preBorderlessLocation = _form.Location;
                        _preBorderlessSize = _form.Size;
                        _preBorderlessScreenArea = _form.RectangleToScreen(clientArea);
                    }

                    System.Drawing.Rectangle dBounds = Screen.GetBounds(_form);

                    _form.WindowState = FormWindowState.Normal;
                    _form.FormBorderStyle = FormBorderStyle.None;
                    _form.TopMost = false;

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

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
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

            base.UpdateDescription(newWidth, newHeight, newDepth, newMipMapCount, newArraySize, newFormat);
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

            if (_mode != _requestedMode)
                UpdateFormMode(_requestedMode);

            if (_loop.NextFrame())
            {
                if (Visible != _form.Visible)
                {
                    if (Visible)
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

        private protected override void OnPipelineDispose()
        {
            if (_swapChain != null)
                _disposing = true;

            base.OnPipelineDispose();
        }

        /// <summary>Gets or sets the form title.</summary>
        public string Title
        {
            get => _form.Text;
            set => _form.Text = value;
        }

        public IntPtr Handle => _formHandle;

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

        /// <summary>
        /// Gets or sets whether or not the form is visible.
        /// </summary>
        public bool Visible { get; set; }
    }
}

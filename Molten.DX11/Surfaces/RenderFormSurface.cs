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
    public class RenderFormSurface : SwapChainSurface, INativeSurface
    {
        RenderLoop _loop;
        RenderForm _control;
        Rectangle _bounds;
        IntPtr _formHandle;
        DisplayMode _displayMode;
        string _title;
        string _ctrlName;
        bool _disposing;
        bool _focused;
        bool _propertiesDirty;

        System.Drawing.Size? _preBorderlessSize;
        System.Drawing.Point? _preBorderlessLocation;
        System.Drawing.Rectangle? _preBorderlessScreenArea;

        WindowMode _mode = WindowMode.Windowed;
        WindowMode _requestedMode = WindowMode.Windowed;

        public event WindowSurfaceHandler OnClose;

        public event WindowSurfaceHandler OnMinimize;

        public event WindowSurfaceHandler OnRestore;

        public event WindowSurfaceHandler OnHandleChanged;

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
        internal RenderFormSurface(string formTitle, string formName, RendererDX11 renderer, int mipCount = 1, int sampleCount = 1)
            : base(renderer, mipCount, sampleCount)
        {
            _title = formTitle;
            _ctrlName = formName;
        }

        protected override void OnSwapChainMissing()
        {
            _control = new RenderForm(_title);
            _control.WindowState = FormWindowState.Maximized;
            _formHandle = _control.Handle;
            OnHandleChanged?.Invoke(this);

            _loop = new RenderLoop(_control)
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
            CreateSwapChain(_displayMode, true, _control.Handle);

            // Subscribe to all the needed form events
            _control.AllowUserResizing = true;
            _control.UserResized += _form_Resized;
            _control.Move += _form_Moved;
            _control.FormClosing += _form_FormClosing;
            _control.GotFocus += _form_GotFocus;
            _control.LostFocus += _form_LostFocus;

            _control.KeyDown += (sender, args) =>
            {
                if (args.Alt)
                    args.Handled = true;
            };

            // Ignore all windows events
            Device.DisplayManager.DxgiFactory.MakeWindowAssociation(_control.Handle, WindowAssociationFlags.IgnoreAltEnter);
        }

        private void _form_LostFocus(object sender, EventArgs e)
        {
            _focused = false;
            OnFocusLost?.Invoke(this);
        }

        private void _form_GotFocus(object sender, EventArgs e)
        {
            _focused = true;
            OnFocusGained?.Invoke(this);
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
                w = _control.Bounds.Width;
                h = _control.Bounds.Height;
            }
            else
            {
                w = _control.ClientSize.Width;
                h = _control.ClientSize.Height;
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
            System.Drawing.Rectangle clientArea = _control.ClientRectangle;

            // Handle new mode
            switch (_mode)
            {
                case WindowMode.Windowed:
                    _control.FormBorderStyle = FormBorderStyle.Sizable;

                    // Calculate offset due to borders and title bars, based on the current mode of the window.
                    if (_preBorderlessLocation != null && _preBorderlessSize != null)
                    {
                        _control.Move -= _form_Moved;
                        _control.Location = _preBorderlessLocation.Value;
                        _control.Size = _preBorderlessSize.Value;
                        System.Drawing.Rectangle screenArea = _preBorderlessScreenArea.Value;
                        _control.Move += _form_Moved;

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

                        System.Drawing.Rectangle screenArea = _control.RectangleToScreen(clientArea);
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
                        _preBorderlessLocation = _control.Location;
                        _preBorderlessSize = _control.Size;
                        _preBorderlessScreenArea = _control.RectangleToScreen(clientArea);
                    }

                    System.Drawing.Rectangle dBounds = Screen.GetBounds(_control);

                    _control.WindowState = FormWindowState.Normal;
                    _control.FormBorderStyle = FormBorderStyle.None;
                    _control.TopMost = false;

                    _control.Bounds = dBounds;
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
                DisposeObject(ref _control);
                return false;
            }

            if (_propertiesDirty)
            {
                if (_mode != _requestedMode)
                    UpdateFormMode(_requestedMode);

                _control.Name = _ctrlName;
                _control.Text = _title;
                _propertiesDirty = false;
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

        /// <summary>Gets or sets the form title.</summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                _propertiesDirty = true;
            }
        }

        /// <summary>Gets or sets the form name.</summary>
        public string Name
        {
            get => _ctrlName;
            set
            {
                _ctrlName = value;
                _propertiesDirty = true;
            }
        }

        public bool IsFocused => _focused;

        public IntPtr Handle => _formHandle;

        /// <summary>Gets or sets the mode of the output form.</summary>
        public WindowMode Mode
        {
            get => _requestedMode;
            set
            {
                _requestedMode = value;
                _propertiesDirty = true;
            }
        }

        /// <summary>Gets the bounds of the window surface.</summary>
        public Rectangle Bounds => _bounds;

        /// <summary>
        /// Gets or sets whether or not the form is visible.
        /// </summary>
        public bool Visible { get; set; }
    }
}

using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Molten.Graphics
{
    public abstract class WinformsSurface<T> : SwapChainSurface
        where T : Control
    {
        T _control;
        Control _parent;
        IntPtr _handle;
        IntPtr? _parentHandle;

        RenderLoop _loop;
        protected Rectangle _bounds;
        DisplayMode _displayMode;
        string _title;
        string _ctrlName;
        bool _disposing;
        bool _focused;
        bool _propertiesDirty = true;

        protected WindowMode _mode = WindowMode.Windowed;
        WindowMode _requestedMode = WindowMode.Windowed;

        internal WinformsSurface(string controlTitle, string controlName, RendererDX11 renderer, int mipCount, int sampleCount) : base(renderer, mipCount, sampleCount)
        {
            _title = controlTitle;
            _ctrlName = controlName;
        }

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
        {
            if (_displayMode.Width != newWidth || _displayMode.Height != newHeight)
            {
                _displayMode.Width = newWidth;
                _displayMode.Height = newHeight;

                // TODO validate display mode here. If invalid or unsupported by display, choose nearest supported.

                UpdateControlMode(_control, _mode);
                _swapChain.ResizeTarget(ref _displayMode.Description);
                Device.Log.WriteLine($"{typeof(T)} surface '{_ctrlName}' resized to {newWidth}x{newHeight}");
            }
            else
            {
                UpdateControlMode(_control, _mode);
            }

            base.UpdateDescription(newWidth, newHeight, newDepth, newMipMapCount, newArraySize, newFormat);
        }

        protected abstract void UpdateControlMode(T control, WindowMode mode);

        protected override void OnSwapChainMissing()
        {
            CreateControl(_title, ref _control, ref _handle);

            _loop = new RenderLoop(_control)
            {
                UseApplicationDoEvents = false,
            };

            //set default bounds
            UpdateControlMode(_control, _requestedMode);

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

            SubscribeToControl(_control);

            _control.KeyDown += (sender, args) =>
            {
                if (args.Alt)
                    args.Handled = true;
            };

            // Ignore all windows events
            Device.DisplayManager.DxgiFactory.MakeWindowAssociation(_control.Handle, WindowAssociationFlags.IgnoreAltEnter);
        }

        protected abstract void CreateControl(string title, ref T control, ref IntPtr handle);

        protected abstract void SubscribeToControl(T control);

        protected abstract void DisposeControl();

        protected abstract void OnNewParent(Control newParent, T control);

        protected override bool OnPresent()
        {
            if (_disposing)
            {
                DisposeObject(ref _loop);
                DisposeObject(ref _swapChain);
                DisposeControl();
                return false;
            }

            if (_propertiesDirty)
            {
                if (_mode != _requestedMode)
                    UpdateControlMode(_control, _requestedMode);

                if (_parent != _control.Parent)
                {
                    _control.Parent = _parent;
                    OnNewParent(_parent, _control);
                }

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

        private protected override void OnPipelineDispose()
        {
            _disposing = true;
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

        public bool IsFocused
        {
            get => _focused;
            protected set
            {
                if(_focused != value)
                {
                    _focused = value;
                    _propertiesDirty = true;
                }
            }
        }

        public IntPtr Handle => _handle;

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

        /// <summary>
        /// [Internal] Gets the control.
        /// </summary>
        internal T Control => _control;

        /// <summary>
        /// Gets or sets the control's handle.
        /// </summary>
        public IntPtr? ParentHandle
        {
            get => _parentHandle;
            set
            {
                if(_parentHandle != value)
                {
                    _parentHandle = value;

                    if (_parentHandle.HasValue && _parentHandle.Value != IntPtr.Zero)
                    {
                        _parent = System.Windows.Forms.Control.FromHandle(_parentHandle.Value);
                    }
                    else
                    {
                        _parent = null;
                    }

                    _propertiesDirty = true;
                }
            }
        }

        protected Control Parent => _parent;
    }
}

using Molten.Graphics.Dxgi;
using Molten.Windows32;
using Silk.NET.DXGI;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    public unsafe abstract class WinformsSurface<T> : SwapChainSurface
        where T : Control
    {
        T _control;
        Control _parent;
        IntPtr _handle;
        IntPtr? _parentHandle;

        protected Rectangle _bounds;
        DisplayMode _displayMode;
        string _title;
        string _ctrlName;
        bool _disposing;
        bool _focused;
        bool _propertiesDirty = true;

        protected WindowMode _mode = WindowMode.Windowed;
        WindowMode _requestedMode = WindowMode.Windowed;

        internal WinformsSurface(string controlTitle, string controlName, RendererDX11 renderer, uint mipCount) : 
            base(renderer, mipCount)
        {
            _title = controlTitle;
            _ctrlName = controlName;
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            if (_displayMode.Width != newWidth || _displayMode.Height != newHeight)
            {
                _displayMode.Width = newWidth;
                _displayMode.Height = newHeight;

                // TODO validate display mode here. If invalid or unsupported by display, choose nearest supported.

                UpdateControlMode(_control, _mode);

                fixed(ModeDesc1* ptrMode = &_displayMode.Description)
                    NativeSwapChain->ResizeTarget((ModeDesc*)ptrMode);

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
            CreateControl(_title, out _control, out _handle);

            //set default bounds
            UpdateControlMode(_control, _requestedMode);

            ModeDesc1 modeDesc = new ModeDesc1()
            {
                Width = (uint)_bounds.Width,
                Height = (uint)_bounds.Height,
                RefreshRate = new Rational(60, 1),
                Format = DxgiFormat,
                Scaling = ModeScaling.Stretched,
                ScanlineOrdering = ModeScanlineOrder.Progressive,
            };

            _displayMode = new DisplayMode(ref modeDesc);
            CreateSwapChain(_displayMode, true, _control.Handle);

            SubscribeToControl(_control);

            _control.KeyDown += (sender, args) =>
            {
                if (args.Alt)
                    args.Handled = true;
            };

            // Ignore all windows events
            Device.DisplayManager.DxgiFactory->MakeWindowAssociation(_control.Handle, (uint)WindowAssociationFlags.NoAltEnter);
        }

        protected abstract void CreateControl(string title, out T control, out IntPtr handle);

        protected abstract void SubscribeToControl(T control);

        protected abstract void DisposeControl();

        protected abstract void OnNewParent(Control newParent, T control);

        protected override bool OnPresent()
        {
            if (_disposing)
            {
                SilkUtil.ReleasePtr(ref NativeSwapChain);
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

            if (NextFrame())
            {
                if (IsVisible != _control.Visible)
                {
                    if (IsVisible)
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

        /// <summary>
        /// Returns whether or not the next frame should be processed and handles native window event dispatching.
        /// </summary>
        /// <remarks>This code is taken from: https://github.com/sharpdx/SharpDX/blob/ab36f12303e24aa60fe804866617716b6ded95db/Source/SharpDX.Desktop/RenderLoop.cs#L114</remarks>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private bool NextFrame()
        {
            // TODO replace this method with better Win32 message loop

            bool controlAlive = true;

            if (_handle != IntPtr.Zero)
            {
                // Previous code not compatible with Application.AddMessageFilter but faster then DoEvents
                NativeMessage msg;
                while (Win32.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0) != 0)
                {
                    if (Win32.GetMessage(out msg, IntPtr.Zero, 0, 0) == -1)
                        throw new InvalidOperationException($"An error happened in rendering loop while processing windows messages. Error: {Marshal.GetLastWin32Error()}");

                    // NCDESTROY event?
                    if (msg.msg == 130)
                        controlAlive = false;

                    var message = new Message() { HWnd = msg.handle, LParam = msg.lParam, Msg = (int)msg.msg, WParam = msg.wParam };
                    if (!Application.FilterMessage(ref message))
                    {
                        Win32.TranslateMessage(ref msg);
                        Win32.DispatchMessage(ref msg);
                    }
                }
            }
            else 
            {
                controlAlive = false;
            }

            return controlAlive;
        }

        internal override void PipelineRelease()
        {
            base.PipelineRelease();

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
        public new string Name
        {
            get => _ctrlName;
            set
            {
                base.Name = $"Winforms surface - {value}";
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
        public Rectangle RenderBounds => _bounds;

        /// <summary>
        /// Gets or sets whether or not the form is visible.
        /// </summary>
        public bool IsVisible { get; set; }

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

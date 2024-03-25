using Molten.Graphics.Dxgi;
using Molten.Windows32;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Message = System.Windows.Forms.Message;

namespace Molten.Graphics.DX12;

public class FormSurfaceDX12 : SwapChainSurfaceDX12, INativeSurface
{
    delegate void FormMessageCallback(ref Message m);

    class SurfaceForm : Form
    {
        FormMessageCallback _callback;

        internal SurfaceForm(FormMessageCallback callback)
        {
            _callback = callback;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == (Keys.Menu | Keys.Alt) || keyData == Keys.F10)
                return true;

            return base.ProcessDialogKey(keyData);
        }

        protected override void WndProc(ref Message m)
        {
            _callback(ref m);
            base.WndProc(ref m);
        }
    }

    public event WindowSurfaceHandler OnClose;

    public event WindowSurfaceHandler OnMaximize;

    public event WindowSurfaceHandler OnMinimize;

    public event WindowSurfaceHandler OnRestore;

    public event WindowSurfaceHandler OnHandleChanged;

    public event WindowSurfaceHandler OnParentChanged;

    public event WindowSurfaceHandler OnFocusGained;

    public event WindowSurfaceHandler OnFocusLost;

    Rectangle _bounds;
    SurfaceForm _form;
    Control _parent;
    nint? _parentHandle;
    bool _propertiesDirty = true;

    string _title;
    string _ctrlName;
    bool _focused;
    bool _disposing;
    Size _pendingSize;
    Size _normalSize;
    TextureDimensions _requestedTexDim;
    FormWindowState _prevWindowState;

    DisplayModeDXGI _displayMode;
    WindowMode _curMode = WindowMode.Windowed;
    WindowMode _requestedMode = WindowMode.Windowed;

    System.Drawing.Size? _preBorderlessSize;
    System.Drawing.Point? _preBorderlessLocation;
    System.Drawing.Rectangle? _preBorderlessScreenArea;

    public FormSurfaceDX12(DeviceDX12 device, uint width, uint height, uint mipCount, string title, string controlName,
        GpuResourceFormat format = GpuResourceFormat.B8G8R8A8_UNorm) : 
        base(device, width, height, mipCount, format)
    {
        _title = title;
        _ctrlName = controlName;
        _prevWindowState = FormWindowState.Normal;
    }

    private void WndProc(ref Message m)
    {
        nint wparam = m.WParam;

        WndMessageType mType = (WndMessageType)m.Msg;
        /*switch (mType)
        {
            case WndMessageType.WM_SIZE:
                WndSizeType sizeType = (WndSizeType)wparam;
                switch (sizeType)
                {
                    default:
                        break;
                }


                break;
        }*/
    }

    protected override void OnCreateSwapchain(ref ResourceDesc1 desc)
    {
        _requestedTexDim = Dimensions;

        GraphicsManagerDXGI manager = Device.Manager as GraphicsManagerDXGI; 
        _form = new SurfaceForm(WndProc);
        _form.Size = new System.Drawing.Size((int)desc.Width, (int)desc.Height);
        OnHandleChanged?.Invoke(this);

        _form.Resize += _form_Resize;
        _form.Move += _form_Moved;
        _form.FormClosing += _form_FormClosing;
        _form.GotFocus += _form_GotFocus;
        _form.LostFocus += _form_LostFocus;
        _form.ResizeBegin += _form_ResizeBegin;
        _form.ResizeEnd += _form_ResizeEnd;

        UpdateFormMode();

        ModeDesc1 modeDesc = new ModeDesc1()
        {
            Width = (uint)_bounds.Width,
            Height = (uint)_bounds.Height,
            RefreshRate = new Rational(60, 1),
            Format = DxgiFormat,
            Scaling = ModeScaling.Stretched,
            ScanlineOrdering = ModeScanlineOrder.Progressive,
        };

        desc.Width = modeDesc.Width;
        desc.Height = modeDesc.Height;

        _displayMode = new DisplayModeDXGI(ref modeDesc);
        CreateSwapChain(_displayMode, _form.Handle);

        _form.KeyDown += (sender, args) =>
        {
            if (args.Alt)
                args.Handled = true;
        };

        // Ignore all windows events
        unsafe
        {
            manager.DxgiFactory->MakeWindowAssociation(_form.Handle, (uint)WindowAssociationFlags.NoAltEnter);
        }
    }

    protected override void OnResizeTextureImmediate(ref readonly TextureDimensions dimensions, GpuResourceFormat format)
    {
        base.OnResizeTextureImmediate(dimensions, format);
        RequestFormResize(dimensions.Width, dimensions.Height);
    }

    private void RequestFormResize(uint newWidth, uint newHeight)
    {
        _propertiesDirty = true;
        _pendingSize = new Size((int)newWidth, (int)newHeight);
    }

    private void UpdateFormMode()
    {
        if (_curMode != _requestedMode)
            Device.Log.WriteLine($"Form surface '{Name}' mode set to '{_requestedMode}'");

        // Update current mode
        _curMode = _requestedMode;
        System.Drawing.Rectangle clientArea = _form.ClientRectangle;

        // Handle new mode
        switch (_curMode)
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

    private void _form_ResizeEnd(object sender, EventArgs e)
    {
        _form_Resize(sender, e);
        _form.Resize += _form_Resize;
    }

    private void _form_ResizeBegin(object sender, EventArgs e)
    {
        _form.Resize -= _form_Resize;
    }

    private void _form_Resize(object sender, EventArgs e)
    {
        TextureDimensions dim = Dimensions;
        FormWindowState state = _form.WindowState;
        if (_prevWindowState != state)
        {
            switch (state)
            {
                case FormWindowState.Normal:
                    OnRestore?.Invoke(this);
                    break;

                case FormWindowState.Minimized:
                    OnMinimize?.Invoke(this);
                    break;

                case FormWindowState.Maximized:
                    OnMaximize?.Invoke(this);
                    break;
            }

            _prevWindowState = state;
        }

        // Don't resize texture if minimized. It may end up with 0 dimensions, which is invalid.
        if (state == FormWindowState.Minimized)
            return;

        if (Mode == WindowMode.Borderless)
        {
            dim.Width = (uint)_form.Bounds.Width;
            dim.Height = (uint)_form.Bounds.Height;
        }
        else
        {
            dim.Width = (uint)_form.ClientSize.Width;
            dim.Height = (uint)_form.ClientSize.Height;
        }

        // Request a texture resize to be done in the next Present().
        _requestedTexDim = dim;
        _propertiesDirty = true;
    }

    void _form_FormClosing(object sender, FormClosingEventArgs e)
    {
        e.Cancel = true;
        OnClose?.Invoke(this);
    }

    void _form_Moved(object sender, EventArgs e)
    {
        UpdateFormMode();
    }

    private void _form_LostFocus(object sender, EventArgs e)
    {
        IsFocused = false;
        OnFocusLost?.Invoke(this);
    }

    private void _form_GotFocus(object sender, EventArgs e)
    {
        IsFocused = true;
        OnFocusGained?.Invoke(this);
    }

    public void Close()
    {
        _form?.Close();
    }

    private void OnNewParent()
    {
        if (_parent is Form pForm) // TODO what if we want to render into a control with a parent that is not a form?
        {
            _form.MdiParent = pForm;
            OnParentChanged?.Invoke(this);
        }
        else
        {
            throw new InvalidOperationException("WindowsFormSurface cannot be parented to a non-form or non-window control.");
        }
    }

    private void DisposeForm()
    {
        if (_parent != null)
            _parent.Move -= _form_Moved;

        _form.Resize -= _form_Resize;
        _form.ResizeBegin -= _form_ResizeBegin;
        _form.ResizeEnd -= _form_ResizeEnd;
        _form.Move -= _form_Moved;
        _form.FormClosing -= _form_FormClosing;
        _form.GotFocus -= _form_GotFocus;
        _form.LostFocus -= _form_LostFocus;

        ParentHandle = null;
        _form?.Dispose();
    }

    protected override bool OnPresent()
    {
        if (_disposing)
        {
            unsafe
            {
                NativeUtil.ReleasePtr(ref SwapChainHandle);
            }

            DisposeForm();
            return false;
        }


        // Apply requested form size.
        if (_pendingSize != _normalSize)
        {
            if (_form.WindowState == FormWindowState.Normal)
            {
                _form.Size = _pendingSize; // Set this first so the form can provide us the actual size it will use.
                _normalSize = _form.Size; // Use form's actual size.
                _pendingSize = _normalSize;
            }

            _normalSize = _pendingSize;
        }

        if (_requestedTexDim.Width != Width || _requestedTexDim.Height != Height)
        {
            uint fbMax = Device.FrameBufferSize;
            uint fbIndex = Math.Min(Device.FrameBufferIndex, fbMax - 1);
            base.OnResizeTextureImmediate(ref _requestedTexDim, ResourceFormat);
            InvokeOnResize();
        }

        // Update window mode.
        if (_curMode != _requestedMode)
            UpdateFormMode();

        // Update parent.
        if (_parent != _form.Parent)
        {
            _form.Parent = _parent;
            OnNewParent();
            _parent.Move += _form_Moved;
        }

        // Update basic form properties
        _form.Name = _ctrlName;
        _form.Text = _title;

        _propertiesDirty = false;

        if (NextFrame())
        {
            if (IsVisible != _form.Visible)
            {
                if (IsVisible)
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

        if (_form.Handle != 0)
        {
            // Previous code not compatible with Application.AddMessageFilter but faster then DoEvents
            NativeMessage msg;
            while (Win32.PeekMessage(out msg, _form.Handle, 0, 0, 0) != 0)
            {
                if (Win32.GetMessage(out msg, _form.Handle, 0, 0) == -1)
                    throw new InvalidOperationException($"An error happened in rendering loop while processing windows messages. Error: {Marshal.GetLastWin32Error()}");

                // NCDESTROY event?
                if (msg.msg == 130)
                    controlAlive = false;

                Message message = new Message()
                {
                    HWnd = msg.handle,
                    LParam = msg.lParam,
                    Msg = (int)msg.msg,
                    WParam = msg.wParam
                };

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

    public nint? WindowHandle => _form?.Handle;

    /// <summary>
    /// Gets or sets the control's handle.
    /// </summary>
    public nint? ParentHandle
    {
        get => _parentHandle;
        set
        {
            if (_parentHandle != value)
            {
                _parentHandle = value;

                if (_parentHandle.HasValue && _parentHandle.Value != IntPtr.Zero)
                    _parent = System.Windows.Forms.Control.FromHandle(_parentHandle.Value);
                else
                    _parent = null;

                _propertiesDirty = true;
            }
        }
    }

    public bool IsFocused
    {
        get => _focused;
        protected set
        {
            if (_focused != value)
            {
                _focused = value;
                _propertiesDirty = true;
            }
        }
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
}

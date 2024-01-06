using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal unsafe class WindowSurfaceVK : SwapChainSurfaceVK, IWindow
{
    WindowHandle* _window;
    string _title;
    bool _visible;

    internal unsafe WindowSurfaceVK(DeviceVK device, string title, uint width, uint height, uint mipCount,
        GraphicsResourceFlags flags = GraphicsResourceFlags.None,
        GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm,
        PresentModeKHR presentMode = PresentModeKHR.ImmediateKhr,
        string name = null) : 
        base(device, title, width, height, mipCount, flags, format, presentMode, name)
    {
        _title = title;
        _visible = true;
    }

    protected unsafe override Result OnCreateSurface(RendererVK renderer, int width, int height, out VkNonDispatchableHandle result)
    {
        result = new VkNonDispatchableHandle();
        renderer.GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        renderer.GLFW.WindowHint(WindowHintBool.Resizable, true);
        _window = renderer.GLFW.CreateWindow(width, height, _title, null, null);

        if (_window == null)
        {
            renderer.Log.Error($"Failed to create {width} x {height} window for WindowSurfaceVK");
            return Result.ErrorInitializationFailed;
        }

        VkHandle instanceHandle = new VkHandle(renderer.Instance->Handle);
        Result r = Result.Success;

        fixed (VkNonDispatchableHandle* ptrResult = &result)
            r = (Result)renderer.GLFW.CreateWindowSurface(instanceHandle, _window, null, ptrResult);

        // Ensure window is visible.
        if(r.Check(renderer))
            renderer.GLFW.ShowWindow(_window);

        return r;
    }

    protected override void OnDestroySurface(RendererVK renderer)
    {
        renderer.GLFW.DestroyWindow(_window);
        _window = null;
    }

    protected override void OnGraphicsRelease()
    {
        base.OnGraphicsRelease();

        if (_window != null)
            (Device.Renderer as RendererVK).GLFW.DestroyWindow(_window);
    }

    public override void Dispatch(Action callback)
    {
        throw new NotImplementedException();
    }

    public override void Close()
    {
        if(_window != null)
            (Device.Renderer as RendererVK).GLFW.SetWindowShouldClose(_window, true);
    }

    public override bool IsFocused => throw new NotImplementedException();

    public override nint? WindowHandle => (nint)_window;

    /// <inheritdoc/>
    public override string Title
    {
        get => _title;
        set
        {
            if (_window != null)
                (Device.Renderer as RendererVK).GLFW.SetWindowTitle(_window, _title);
        }
    }

    public override bool IsVisible
    {
        get => _visible;
        set
        {
            if(_visible != value)
            {
                _visible = value;
                RendererVK renderer = Device.Renderer as RendererVK;
                if (_visible)
                    renderer.GLFW.ShowWindow(_window);
                else
                    renderer.GLFW.HideWindow(_window);
            }
        }
    }
}

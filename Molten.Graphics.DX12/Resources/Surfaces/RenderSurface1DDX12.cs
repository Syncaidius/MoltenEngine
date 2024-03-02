using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class RenderSurface1DDX12 : Texture1DDX12, IRenderSurface1D
{
    internal RenderSurface1DDX12(
        DeviceDX12 device,
        uint width,
        GraphicsResourceFlags flags = GraphicsResourceFlags.None,
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
        uint mipCount = 1,
        uint arraySize = 1, 
        string name = null,
        ProtectedSessionDX12 protectedSession = null)
        : base(device, width, flags, format, mipCount, arraySize, name, protectedSession)
    {
        Viewport = new ViewportF(0, 0, width, 1);
        Name = $"Surface_{name ?? GetType().Name}";
    }

    protected override ID3D12Resource1* OnCreateTexture()
    {       
        Viewport = new ViewportF(Viewport.X, Viewport.Y, Desc.Width, Desc.Height);
        return base.OnCreateTexture();
    }

    protected virtual void SetRTVDescription(ref RenderTargetViewDesc desc) { }

    protected override unsafe ResourceHandleDX12 OnCreateHandle(ID3D12Resource1* ptr)
    {
        RenderTargetViewDesc desc = new RenderTargetViewDesc()
        {
            Format = Desc.Format,
            ViewDimension = RtvDimension.Texture1Darray,
            Texture1DArray = new Tex1DArrayRtv()
            {
                ArraySize = Desc.DepthOrArraySize,
                MipSlice = 0,
                FirstArraySlice = 0,
            },
        };

        SetRTVDescription(ref desc);
        RTHandleDX12 handle = new RTHandleDX12(this, ptr);
        handle.RTV.Initialize(ref desc);

        return handle;
    }

    public void Clear(GraphicsPriority priority, Color color)
    {
        if (priority == GraphicsPriority.Immediate)
        {
            Apply(Device.Queue);
            Device.Queue.Clear(this, color);
        }
        else
        {
            Surface1DClearTaskDX12 task = Device.Tasks.Get<Surface1DClearTaskDX12>();
            task.Color = color;
            Device.Tasks.Push(priority, this, task);
        }
    }

    /// <summary>Gets the viewport that defines the default renderable area of the render target.</summary>
    public ViewportF Viewport { get; protected set; }
}

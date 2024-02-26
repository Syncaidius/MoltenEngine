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

    protected override void OnCreateResource()
    {
        base.OnCreateResource();

        Viewport = new ViewportF(Viewport.X, Viewport.Y, Desc.Width, Desc.Height); 
        ResourceHandleDX12<RenderTargetViewDesc> handle = (ResourceHandleDX12<RenderTargetViewDesc>)Handle;
        Device.Heap.Allocate(handle.View);
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

        return new ResourceHandleDX12<RenderTargetViewDesc>(this, ptr, ref desc);
    }

    public void Clear(GraphicsPriority priority, Color color)
    {
        Surface1DClearTaskDX12 task = Device.Tasks.Get<Surface1DClearTaskDX12>();
        task.Color = color;
        Device.Tasks.Push(priority, this, task);
    }

    /// <summary>Gets the viewport that defines the default renderable area of the render target.</summary>
    public ViewportF Viewport { get; protected set; }
}

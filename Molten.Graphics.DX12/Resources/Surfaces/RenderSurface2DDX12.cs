using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class RenderSurface2DDX12 : Texture2DDX12, IRenderSurface2D
{
    internal RenderSurface2DDX12(
        DeviceDX12 device,
        uint width,
        uint height,
        GraphicsResourceFlags flags = GraphicsResourceFlags.None,
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
        uint mipCount = 1,
        uint arraySize = 1,
        AntiAliasLevel aaLevel = AntiAliasLevel.None,
        MSAAQuality msaa = MSAAQuality.Default, 
        string name = null,
        ProtectedSessionDX12 protectedSession = null)
        : base(device, width, height, flags, format, mipCount, arraySize, aaLevel, msaa, name, protectedSession)
    {
        Viewport = new ViewportF(0, 0, width, height);
        Name = $"Surface_{name ?? GetType().Name}";
    }

    protected override void OnCreateResource()
    {
        base.OnCreateResource();

        Viewport = new ViewportF(Viewport.X, Viewport.Y, Desc.Width, Desc.Height);
    }

    protected override unsafe ResourceHandleDX12 OnCreateHandle(ID3D12Resource1* ptr)
    {
        RenderTargetViewDesc desc = new RenderTargetViewDesc()
        {
            Format = Desc.Format,
            ViewDimension = RtvDimension.Texture2Darray,
            Texture2DArray = new Tex2DArrayRtv()
            {
                ArraySize = Desc.DepthOrArraySize,
                MipSlice = 0,
                FirstArraySlice = 0,
                PlaneSlice = 0,
            },
        };

        return new ResourceHandleDX12<RenderTargetViewDesc>(this, ptr, ref desc);
    }

    public void Clear(GraphicsPriority priority, Color color)
    {
        Surface2DClearTaskDX12 task = Device.Tasks.Get<Surface2DClearTaskDX12>();
        task.Color = color;
        Device.Tasks.Push(priority, this, task);
    }

    /// <summary>Gets the viewport that defines the default renderable area of the render target.</summary>
    public ViewportF Viewport { get; protected set; }
}

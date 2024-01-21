using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

public unsafe class RenderSurface1DDX11 : Texture1DDX11, IRenderSurface1D
{
    RTViewDX11[] _rtvs;

    internal RenderSurface1DDX11(
        DeviceDX11 device,
        uint width,
        GraphicsResourceFlags flags = GraphicsResourceFlags.None,
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
        uint mipCount = 1,
        uint arraySize = 1, 
        string name = null)
        : base(device, width, flags, format, mipCount, arraySize, name)
    {
        Viewport = new ViewportF(0, 0, width, 1);
        Name = $"Surface_{name ?? GetType().Name}";
    }

    protected override ResourceHandleDX11<ID3D11Resource> CreateTexture(DeviceDX11 device)
    {
        fixed (Texture1DDesc* pDesc = &Desc)
            Device.Ptr->CreateTexture1D(pDesc, null, ref NativeTexture);

        SurfaceHandleDX11 handle = new SurfaceHandleDX11(this)
        {
            NativePtr = (ID3D11Resource*)NativeTexture,
        };

        ref RenderTargetViewDesc1 desc = ref handle.RTV.Desc;
        desc.Format = DxgiFormat;

        SetRTVDescription(ref desc);

        desc.ViewDimension = RtvDimension.Texture1Darray;
        desc.Texture1DArray = new Tex1DArrayRtv()
        {
            ArraySize = Desc.ArraySize,
            MipSlice = 0,
            FirstArraySlice = 0,
        };

        handle.RTV.Create();
        return handle;
    }

    protected virtual void SetRTVDescription(ref RenderTargetViewDesc1 desc) { }

    protected override void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat)
    {
        base.UpdateDescription(dimensions, newFormat);

        Desc.MipLevels = 1; // NOTE: Do we set this on render targets?
        Viewport = new ViewportF(Viewport.X, Viewport.Y, dimensions.Width, dimensions.Height);
    }

    internal virtual void OnClear(GraphicsQueueDX11 cmd, Color color)
    {
        SurfaceHandleDX11 rsHandle = Handle as SurfaceHandleDX11;

        if (rsHandle.RTV.Ptr != null)
        {
            Color4 c4 = color;
            cmd.Ptr->ClearRenderTargetView(rsHandle.RTV, (float*)&c4);
        }
    }

    public void Clear(GraphicsPriority priority, Color color)
    {
        Surface1DClearTask task = Device.Tasks.Get<Surface1DClearTask>();
        task.Color = color;
        Device.Tasks.Push(priority, this, task);
    }

    /// <summary>Gets the viewport that defines the default renderable area of the render target.</summary>
    public ViewportF Viewport { get; protected set; }
}

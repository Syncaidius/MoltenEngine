using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11;

/// <summary>A special kind of render surface for use as a depth-stencil buffer.</summary>
public unsafe class DepthSurfaceDX11 : Texture2DDX11, IDepthStencilSurface
{
    ID3D11DepthStencilView* _depthView;
    ID3D11DepthStencilView* _readOnlyView;
    DepthStencilViewDesc _depthDesc;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="device"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="format"></param>
    /// <param name="mipCount"></param>
    /// <param name="arraySize"></param>
    /// <param name="aaLevel"></param>
    /// <param name="msaa"></param>
    /// <param name="flags">Texture flags</param>
    internal DepthSurfaceDX11(DeviceDX11 device,
        uint width, 
        uint height,
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite,
        DepthFormat format = DepthFormat.R24G8,
        uint mipCount = 1, 
        uint arraySize = 1, 
        AntiAliasLevel aaLevel = AntiAliasLevel.None,
        MSAAQuality msaa = MSAAQuality.Default,
        string name = "surface")
        : base(device, width, height, flags, format.ToGraphicsFormat(), mipCount, arraySize, aaLevel, msaa, name)
    {
        DepthFormat = format;
        Desc.ArraySize = arraySize;
        Desc.Format = format.ToGraphicsFormat().ToApi();
        _depthDesc = new DepthStencilViewDesc();
        _depthDesc.Format = format.ToDepthViewFormat().ToApi();

        name ??= "surface";
        Name = $"depth_{name}";

        if (MultiSampleLevel >= AntiAliasLevel.X2)
        {
            _depthDesc.ViewDimension = DsvDimension.Texture2Dmsarray;
            _depthDesc.Flags = 0U; // DsvFlag.None;
            _depthDesc.Texture2DMSArray = new Tex2DmsArrayDsv()
            {
                ArraySize = Desc.ArraySize,
                FirstArraySlice = 0,
            };
        }
        else
        {
            _depthDesc.ViewDimension = DsvDimension.Texture2Darray;
            _depthDesc.Flags = 0U; //DsvFlag.None;
            _depthDesc.Texture2DArray = new Tex2DArrayDsv()
            {
                ArraySize = Desc.ArraySize,
                FirstArraySlice = 0,
                MipSlice = 0,
            };
        }

        UpdateViewport();
    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
    {
        base.SetSRVDescription(ref desc);
        desc.Format = DepthFormat.ToSRVFormat().ToApi();
    }

    private void UpdateViewport()
    {
        Viewport = new ViewportF(0, 0, Desc.Width, Desc.Height);
    }

    private DsvFlag GetReadOnlyFlags()
    {
        switch (DepthFormat)
        {
            default:
            case DepthFormat.R24G8:
                return DsvFlag.Depth | DsvFlag.Stencil;
            case DepthFormat.R32:
                return DsvFlag.Depth;
        }
    }

    protected override ResourceHandleDX11<ID3D11Resource> CreateTexture(DeviceDX11 device)
    {
        NativeUtil.ReleasePtr(ref _depthView);
        NativeUtil.ReleasePtr(ref _readOnlyView);

        Desc.Width = Math.Max(1, Desc.Width);
        Desc.Height = Math.Max(1, Desc.Height);

        // Create render target texture
        ResourceHandleDX11<ID3D11Resource> handle = base.CreateTexture(device);

        _depthDesc.Flags = 0; // DsvFlag.None;
        SubresourceData* subData = null;

        fixed(DepthStencilViewDesc* pDesc = &_depthDesc)
            device.Handle->CreateDepthStencilView(handle, pDesc, ref _depthView);

        // Create read-only depth view for passing to shaders.
        _depthDesc.Flags = (uint)GetReadOnlyFlags();
        fixed (DepthStencilViewDesc* pDesc = &_depthDesc)
            device.Handle->CreateDepthStencilView(handle, pDesc, ref _readOnlyView);
        _depthDesc.Flags = 0U; // (uint)DsvFlag.None;

        return handle;
    }

    protected override void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat)
    {
        base.UpdateDescription(dimensions, newFormat);
        UpdateViewport();
    }

    internal void OnClear(GraphicsQueueDX11 cmd, DepthClearTask task)
    {
        cmd.Ptr->ClearDepthStencilView(_depthView, (uint)task.Flags, task.DepthClearValue, task.StencilClearValue);
    }

    public void Clear(GraphicsPriority priority, DepthClearFlags flags, float depth = 1.0f, byte stencil = 0)
    {
        DepthClearTask task = Device.Tasks.Get<DepthClearTask>();
        task.Flags = flags;
        task.DepthClearValue = depth;
        task.StencilClearValue = stencil;
        Device.Tasks.Push(priority, this, task);
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _depthView);
        NativeUtil.ReleasePtr(ref _readOnlyView);

        base.OnGraphicsRelease();
    }

    /// <summary>Gets the DepthStencilView instance associated with this surface.</summary>
    internal ID3D11DepthStencilView* DepthView => _depthView;

    /// <summary>Gets the read-only DepthStencilView instance associated with this surface.</summary>
    internal ID3D11DepthStencilView* ReadOnlyView => _readOnlyView;

    /// <summary>Gets the depth-specific format of the surface.</summary>
    public DepthFormat DepthFormat { get; }

    /// <summary>Gets the viewport of the <see cref="DepthSurfaceDX11"/>.</summary>
    public ViewportF Viewport { get; private set; }
}

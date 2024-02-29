using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public class DepthSurfaceDX12 : Texture2DDX12, IDepthStencilSurface
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="device"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="flags"></param>
    /// <param name="format"></param>
    /// <param name="mipCount"></param>
    /// <param name="arraySize"></param>
    /// <param name="aaLevel"></param>
    /// <param name="msaa"></param>
    /// <param name="name"></param>
    /// <param name="protectedSession"></param>
    internal DepthSurfaceDX12(DeviceDX12 device,
        uint width,
        uint height,
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite,
        DepthFormat format = DepthFormat.R24G8,
        uint mipCount = 1,
        uint arraySize = 1,
        AntiAliasLevel aaLevel = AntiAliasLevel.None,
        MSAAQuality msaa = MSAAQuality.Default,
        string name = "surface", 
        ProtectedSessionDX12 protectedSession = null) :
        base(device, width, height, flags, format.ToGraphicsFormat(), mipCount, arraySize, aaLevel, msaa, name, protectedSession)
    {
        DepthFormat = format;

        UpdateViewport();
    }

    protected unsafe override ID3D12Resource1* OnCreateTexture()
    {
        UpdateViewport();
        return base.OnCreateTexture();
    }

    private void UpdateViewport()
    {
        Viewport = new ViewportF(0, 0, Desc.Width, Desc.Height);
    }

    public void Clear(GraphicsPriority priority, DepthClearFlags flags, float depthValue = 1, byte stencilValue = 0)
    {
        throw new NotImplementedException();
    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
    {
        base.SetSRVDescription(ref desc);
        desc.Format = DepthFormat.ToSRVFormat().ToApi();
    }

    public DepthFormat DepthFormat { get; }

    public ViewportF Viewport { get; private set; }
}

using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public class Texture1DDX12 : TextureDX12, ITexture1D
{
    public Texture1DDX12(
        DeviceDX12 device,
        uint width,
        GraphicsResourceFlags flags,
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm,
        uint mipCount = 1,
        uint arraySize = 1,
        string name = null,
        ProtectedSessionDX12 protectedSession = null)
        : base(device, ResourceDimension.Texture1D, new TextureDimensions(width, 1, 1, mipCount, arraySize),
              AntiAliasLevel.None, MSAAQuality.Default, format, flags, name, protectedSession)
    {

    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
    {
        
    }

    protected override void SetUAVDescription(ref ShaderResourceViewDesc srvDesc, ref UnorderedAccessViewDesc desc)
    {
        throw new NotImplementedException();
    }
}

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
        AntiAliasLevel aaLevel = AntiAliasLevel.None, 
        MSAAQuality aaQuality = MSAAQuality.Default,
        string name = null,
        ProtectedSessionDX12 protectedSession = null)
        : base(device, ResourceDimension.Texture1D, new TextureDimensions(width, 1, 1, mipCount, arraySize, aaLevel, aaQuality),
            format, flags, name, protectedSession)
    {

    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
    {
        desc.ViewDimension = SrvDimension.Texture1Darray;
        desc.Format = DxgiFormat;
        desc.Texture1DArray = new Tex1DArraySrv()
        {
            MipLevels = Desc.MipLevels,
            ArraySize = Desc.DepthOrArraySize,
            MostDetailedMip = 0,
            FirstArraySlice = 0,
        };
    }

    protected override void SetUAVDescription(ref ShaderResourceViewDesc srvDesc, ref UnorderedAccessViewDesc desc)
    {
        desc.Format = srvDesc.Format;
        desc.ViewDimension = UavDimension.Texture1Darray;
        desc.Buffer = new BufferUav()
        {
            FirstElement = 0,
            NumElements = (uint)(Desc.Width * Desc.DepthOrArraySize),
        };
    }
}

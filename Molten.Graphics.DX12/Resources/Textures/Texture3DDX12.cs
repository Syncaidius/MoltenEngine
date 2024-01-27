using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class Texture3DDX12 : TextureDX12, ITexture3D
{
    /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
    /// of the provided texture in to the new instance.</summary>
    /// <param name="other"></param>
    internal Texture3DDX12(Texture3DDX12 other)
        : this(other, other.Flags)
    { }
    
    /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
    /// of the provided texture in to the new instance.</summary>
    /// <param name="other"></param>
    /// <param name="flags">A set of flags to override those of the provided texture.</param>
    internal Texture3DDX12(Texture3DDX12 other, GraphicsResourceFlags flags)
        : this(other.Device, other.Width, other.Height, other.Depth, flags, other.ResourceFormat, other.MipMapCount)
    { }

    internal Texture3DDX12(
        DeviceDX12 device,
        uint width,
        uint height,
        uint depth,
        GraphicsResourceFlags flags,
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm,
        uint mipCount = 1,
        string name = null)
        : base(device, ResourceDimension.Texture3D, new TextureDimensions(width, height, depth, mipCount, 1), format, flags, name)
    {

    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
    {
            desc.ViewDimension = SrvDimension.Texture3D;
            desc.Texture3D = new Tex3DSrv()
            {
                MipLevels = Desc.MipLevels,
                MostDetailedMip = 0,
                ResourceMinLODClamp = 0
            };
    }

    protected override void SetUAVDescription(ref ShaderResourceViewDesc srvDesc, ref UnorderedAccessViewDesc desc)
    {
        desc.Format = srvDesc.Format;
        desc.ViewDimension = UavDimension.Texture3D;
        
        desc.Texture3D = new Tex3DUav()
        {
            MipSlice = 0,
            FirstWSlice = 0,
            WSize = Desc.DepthOrArraySize,
        };
    }

    public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
    {
        Resize(priority, newWidth, newHeight, ArraySize, newMipMapCount, newDepth, newFormat);
    }
}

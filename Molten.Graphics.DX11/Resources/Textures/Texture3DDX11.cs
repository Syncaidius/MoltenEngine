using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

public unsafe class Texture3DDX11 : TextureDX11, ITexture3D
{
    internal ID3D11Texture3D1* NativeTexture;
    protected Texture3DDesc1 _desc;

    /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
    /// of the provided texture in to the new instance.</summary>
    /// <param name="other"></param>
    internal Texture3DDX11(Texture3DDX11 other)
        : this(other, other.Flags)
    { }
    
    /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
    /// of the provided texture in to the new instance.</summary>
    /// <param name="other"></param>
    /// <param name="flags">A set of flags to override those of the provided texture.</param>
    internal Texture3DDX11(Texture3DDX11 other, GraphicsResourceFlags flags)
        : this(other.Device, other.Width, other.Height, other.Depth, flags, other.ResourceFormat, other.MipMapCount)
    { }

    internal Texture3DDX11(
        DeviceDX11 device,
        uint width,
        uint height,
        uint depth,
        GraphicsResourceFlags flags,
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm,
        uint mipCount = 1,
        string name = null)
        : base(device, new TextureDimensions(width, height, depth, mipCount, 1), format, flags, name)
    {
        _desc = new Texture3DDesc1()
        {
            Width = Math.Max(width, 1),
            Height = Math.Max(height, 1),
            Depth = Math.Max(depth, 1),
            MipLevels = mipCount,
            Format = format.ToApi(),
            BindFlags = (uint)GetBindFlags(),
            CPUAccessFlags = (uint)Flags.ToCpuFlags(),
            Usage = Flags.ToUsageFlags(),
            MiscFlags = (uint)Flags.ToMiscFlags(),
        };
    }

    protected override ResourceHandleDX11<ID3D11Resource> CreateTexture(DeviceDX11 device)
    {
        SubresourceData* subData = null;

        fixed(Texture3DDesc1* pDesc = &_desc)
            Device.Ptr->CreateTexture3D1(pDesc, subData, ref NativeTexture);

        EngineUtil.Free(ref subData);

        return new ResourceHandleDX11<ID3D11Resource>(this, (ID3D11Resource*)NativeTexture);
    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
    {
            desc.ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture3D;
            desc.Texture3D = new Tex3DSrv()
            {
                MipLevels = _desc.MipLevels,
                MostDetailedMip = 0,
            };
    }

    protected override void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc)
    {
        desc.Format = srvDesc.Format;
        desc.ViewDimension = UavDimension.Texture2Darray;
        
        desc.Texture3D = new Tex3DUav()
        {
            MipSlice = 0,
            FirstWSlice = 0,
            WSize = _desc.Depth,
        };

        desc.Buffer = new BufferUav()
        {
            FirstElement = 0,
            NumElements = _desc.Width * _desc.Height * _desc.Depth,
        };
    }

    protected override void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat)
    {
        _desc.Width = dimensions.Width;
        _desc.Height = dimensions.Height; 
        _desc.Depth = dimensions.Depth;
        _desc.MipLevels = dimensions.MipMapCount;
        _desc.Format = newFormat.ToApi();
    }

    public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
    {
        Resize(priority, newWidth, newHeight, ArraySize, newMipMapCount, newDepth, newFormat);
    }
}

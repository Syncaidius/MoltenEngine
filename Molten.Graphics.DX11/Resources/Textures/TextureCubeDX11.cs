using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11;

public unsafe class TextureCubeDX11 : Texture2DDX11, ITextureCube
{
    internal TextureCubeDX11(DeviceDX11 device, uint width, uint height, GraphicsResourceFlags flags, 
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm, uint mipCount = 1, uint cubeCount = 1, string name = null)
        : base(device, width, height, flags, format, mipCount, 6 * cubeCount, AntiAliasLevel.None, MSAAQuality.Default, name)
    {
        CubeCount = cubeCount;

        Desc = new Texture2DDesc1()
        {
            Width = width,
            Height = height,
            MipLevels = mipCount,
            ArraySize = ArraySize,
            Format = format.ToApi(),
            BindFlags = (uint)(flags.Has(GraphicsResourceFlags.DenyShaderAccess) ? BindFlag.None : BindFlag.ShaderResource),
            CPUAccessFlags = (uint)Flags.ToCpuFlags(),
            SampleDesc = new SampleDesc(1, 0),
            Usage = Flags.ToUsageFlags(),
            MiscFlags = (uint)(Flags.ToMiscFlags() | ResourceMiscFlag.Texturecube),
            TextureLayout = 0U, // TextureLayout.None
        };
    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
    {
        desc.Format = DxgiFormat;
        desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexturecubearray;
        desc.TextureCubeArray = new TexcubeArraySrv()
        {
            MostDetailedMip = 0,
            MipLevels = Desc.MipLevels,
            NumCubes = CubeCount,
            First2DArrayFace = 0,
        };
    }

    protected override void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc)
    {
        desc.Format = srvDesc.Format;
        desc.ViewDimension = UavDimension.Texture2Darray;

        desc.Texture2DArray = new Tex2DArrayUav1()
        {
            ArraySize = Desc.ArraySize,
            FirstArraySlice = srvDesc.Texture2DArray.FirstArraySlice,
            MipSlice = 0,
            PlaneSlice = 0
        };
    }

    protected override void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat)
    {
        base.UpdateDescription(dimensions, newFormat);

        Desc.Width = dimensions.Width;
        Desc.Height = dimensions.Height;
        Desc.MipLevels = dimensions.MipMapCount;
        Desc.Format = newFormat.ToApi();
    }

    public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newMipMapCount)
    {
        TextureResizeTask task = Device.Tasks.Get<TextureResizeTask>();
        task.NewFormat = ResourceFormat;
        task.NewDimensions = new TextureDimensions()
        {
            Width = newWidth,
            Height = newHeight,
            MipMapCount = newMipMapCount,
            ArraySize = ArraySize,
            Depth = Depth,
        };
        Device.Tasks.Push<GraphicsTexture, TextureResizeTask>(priority, this, task);
    }

    public void Resize(GraphicsPriority priority, uint newWidth, uint newMipMapCount)
    {
        Resize(priority, newWidth, Height, newMipMapCount);
    }

    /// <summary>Gets information about the texture.</summary>
    internal ref Texture2DDesc1 Description => ref Desc;

    /// <summary>Gets the number of cube maps stored in the texture. This is greater than 1 if the texture is a cube-map array.</summary>
    public uint CubeCount { get; private set; }
}

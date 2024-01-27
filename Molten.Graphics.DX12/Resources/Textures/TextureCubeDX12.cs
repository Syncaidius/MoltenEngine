using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class TextureCubeDX12 : Texture2DDX12, ITextureCube
{
    /// <summary>
    /// The number of expected array slices in a single cube map.
    /// </summary>
    const int CUBE_SLICE_COUNT = 6;

    internal TextureCubeDX12(DeviceDX12 device, uint width, uint height, GraphicsResourceFlags flags, 
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm, uint mipCount = 1, uint cubeCount = 1, uint arraySize = 1, string name = null,
        ProtectedSessionDX12 protectedSession = null)
        : base(device, width, height, flags, format, mipCount, CUBE_SLICE_COUNT * cubeCount * arraySize, AntiAliasLevel.None, MSAAQuality.Default, name, protectedSession)
    {
        CubeCount = cubeCount;
    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
    {
        desc.Format = DxgiFormat;
        desc.ViewDimension = SrvDimension.Texturecubearray;
        desc.TextureCubeArray = new TexcubeArraySrv()
        {
            MostDetailedMip = 0,
            MipLevels = Desc.MipLevels,
            NumCubes = CubeCount,
            First2DArrayFace = 0,
            ResourceMinLODClamp = 0,
        };
    }

    protected override void SetUAVDescription(ref ShaderResourceViewDesc srvDesc, ref UnorderedAccessViewDesc desc)
    {
        desc.Format = srvDesc.Format;
        desc.ViewDimension = UavDimension.Texture2Darray;

        desc.Texture2DArray = new Tex2DArrayUav()
        {
            ArraySize = Desc.DepthOrArraySize,
            FirstArraySlice = srvDesc.Texture2DArray.FirstArraySlice,
            MipSlice = 0,
            PlaneSlice = 0,
        };
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

    /// <summary>Gets the number of cube maps stored in the texture. This is greater than 1 if the texture is a cube-map array.</summary>
    public uint CubeCount { get; private set; }
}

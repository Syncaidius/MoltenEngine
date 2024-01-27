using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class Texture2DDX12 : TextureDX12, ITexture2D
{
    internal Texture2DDX12(
        DeviceDX12 device,
        uint width,
        uint height,
        GraphicsResourceFlags flags,
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm,
        uint mipCount = 1,
        uint arraySize = 1,
        AntiAliasLevel aaLevel = AntiAliasLevel.None,
        MSAAQuality msaa = MSAAQuality.Default,
        string name = null)
        : base(device, ResourceDimension.Texture2D, new TextureDimensions(width, height, 1, mipCount, arraySize, aaLevel, msaa), format, flags, name)
    { }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
    {
        if (Desc.SampleDesc.Count > 1)
        {
            desc.ViewDimension = SrvDimension.Texture2Dmsarray;
            desc.Texture2DMSArray = new Tex2DmsArraySrv()
            {
                ArraySize = Desc.DepthOrArraySize,
                FirstArraySlice = 0,
            };
        }
        else
        {
            if (Dimensions.IsCubeMap)
            {
                desc.ViewDimension = SrvDimension.Texturecubearray;
                desc.TextureCubeArray = new TexcubeArraySrv()
                {
                    MipLevels = Desc.MipLevels,
                    MostDetailedMip = 0,
                    ResourceMinLODClamp = 0,
                    First2DArrayFace = 0,
                    NumCubes = Desc.DepthOrArraySize / 6U,
                };
            }
            else
            {
                desc.ViewDimension = SrvDimension.Texture2Darray;
                desc.Texture2DArray = new Tex2DArraySrv()
                {
                    ArraySize = Desc.DepthOrArraySize,
                    MipLevels = Desc.MipLevels,
                    MostDetailedMip = 0,
                    FirstArraySlice = 0,
                    PlaneSlice = 0,
                };
            }
        }
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
            PlaneSlice = 0
        };
    }

    public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newMipMapCount = 0,
        uint newArraySize = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
    {
        Resize(priority, newWidth, newHeight, newArraySize, newMipMapCount, Depth, newFormat);
    }
}

using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    public unsafe class Texture1DDX11 : TextureDX11, ITexture1D
    {
        internal ID3D11Texture1D* NativeTexture;
        Texture1DDesc _desc;

        internal Texture1DDX11(
            GraphicsDevice device, 
            uint width, 
            GraphicsResourceFlags flags,
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm, 
            uint mipCount = 1, 
            uint arraySize = 1,
            bool allowMipMapGen = false,
            string name = null)
            : base(device, GraphicsTextureType.Texture1D, new TextureDimensions(width, 1, 1, mipCount, arraySize), 
                  AntiAliasLevel.None, MSAAQuality.Default, format, flags, allowMipMapGen, name)
        {
            if (IsBlockCompressed)
                throw new NotSupportedException("1D textures do not supports block-compressed formats.");

            _desc = new Texture1DDesc()
            {
                Width = width,
                MipLevels = mipCount,
                ArraySize = Math.Max(1, arraySize),
                Format = format.ToApi(),
                BindFlags = (uint)GetBindFlags(),
                CPUAccessFlags = (uint)Flags.ToCpuFlags(),
                Usage = Flags.ToUsageFlags(),
                MiscFlags = (uint)Flags.ToMiscFlags(allowMipMapGen),
            };
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
        {
            desc.Format = DxgiFormat;
            desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture1Darray;
            desc.Texture1DArray = new Tex1DArraySrv()
            {
                ArraySize = _desc.ArraySize,
                MipLevels = _desc.MipLevels,
                MostDetailedMip = 0,
                FirstArraySlice = 0,
            };
        }

        protected override void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc)
        {
            desc.Format = srvDesc.Format;
            desc.ViewDimension = UavDimension.Texture1Darray;
            desc.Buffer = new BufferUav()
            {
                FirstElement = 0,
                NumElements = _desc.Width * _desc.ArraySize,
            };
        }

        protected override void CreateTexture(DeviceDX11 device, ResourceHandleDX11<ID3D11Resource> handle)
        {
            SubresourceData* subData = GetImmutableData(_desc.Usage);

            fixed(Texture1DDesc* pDesc = &_desc)
                (Device as DeviceDX11).Ptr->CreateTexture1D(pDesc, subData, ref NativeTexture);

            handle.NativePtr = (ID3D11Resource*)NativeTexture;
        }

        protected override void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat)
        {
            _desc.Width = dimensions.Width;
            _desc.ArraySize = dimensions.ArraySize;
            _desc.MipLevels = dimensions.MipMapCount;
            _desc.Format = newFormat.ToApi();
        }
    }
}

using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class Texture1DDX11 : TextureDX11, ITexture1D
    {
        internal ID3D11Texture1D* NativeTexture;
        Texture1DDesc _desc;

        internal Texture1DDX11(
            RenderService renderer, 
            uint width, 
            GraphicsResourceFlags flags,
            Format format = Format.FormatR8G8B8A8Unorm, 
            uint mipCount = 1, 
            uint arraySize = 1,
            bool allowMipMapGen = false,
            string name = null)
            : base(renderer, width, 1, 1, mipCount, arraySize, AntiAliasLevel.None, MSAAQuality.Default, format, flags, allowMipMapGen, name)
        {
            if (IsBlockCompressed)
                throw new NotSupportedException("1D textures do not supports block-compressed formats.");

            _desc = new Texture1DDesc()
            {
                Width = width,
                MipLevels = mipCount,
                ArraySize = Math.Max(1, arraySize),
                Format = format,
                BindFlags = (uint)GetBindFlags(),
                CPUAccessFlags = (uint)Flags.ToCpuFlags(),
                Usage = Flags.ToUsageFlags(),
                MiscFlags = (uint)Flags.ToMiscFlags(allowMipMapGen),
            };
        }

        public Texture1DProperties Get1DProperties()
        {
            return new Texture1DProperties()
            {
                Width = Width,
                ArraySize = ArraySize,
                Flags = Flags,
                Format = DataFormat,
                MipMapLevels = MipMapCount,
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

        protected override ID3D11Resource* CreateResource(bool resize)
        {
            SubresourceData* subData = null;
            (Device as DeviceDX11).Ptr->CreateTexture1D(ref _desc, subData, ref NativeTexture);
            return (ID3D11Resource*)NativeTexture;
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            _desc.Width = newWidth;
            _desc.ArraySize = newArraySize;
            _desc.MipLevels = newMipMapCount;
            _desc.Format = newFormat;
        }
    }
}

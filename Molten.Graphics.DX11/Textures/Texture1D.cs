using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class Texture1D : TextureBase, ITexture
    {
        internal ID3D11Texture1D* NativeTexture;
        Texture1DDesc _description;

        internal Texture1D(
            RendererDX11 renderer, 
            uint width, 
            Format format = Format.FormatR8G8B8A8Unorm, 
            uint mipCount = 1, 
            uint arraySize = 1,
            TextureFlags flags = TextureFlags.None,
            string name = null)
            : base(renderer, width, 1, 1, mipCount, arraySize, AntiAliasLevel.None, MSAAQuality.Default, format, flags, name)
        {
            if (IsBlockCompressed)
                throw new NotSupportedException("1D textures do not supports block-compressed formats.");

            _description = new Texture1DDesc()
            {
                Width = width,
                MipLevels = mipCount,
                ArraySize = Math.Max(1, arraySize),
                Format = format,
                BindFlags = (uint)GetBindFlags(),
                CPUAccessFlags = (uint)GetAccessFlags(),
                Usage = GetUsageFlags(),
                MiscFlags = (uint)GetResourceFlags(),
            };
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
        {
            desc.Format = DxgiFormat;
            desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture1Darray;
            desc.Texture1DArray = new Tex1DArraySrv()
            {
                ArraySize = _description.ArraySize,
                MipLevels = _description.MipLevels,
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
                NumElements = _description.Width * _description.ArraySize,
            };
        }

        protected override ID3D11Resource* CreateResource(bool resize)
        {
            SubresourceData* subData = null;
            (Device as DeviceDX11).Ptr->CreateTexture1D(ref _description, subData, ref NativeTexture);
            return (ID3D11Resource*)NativeTexture;
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            _description.Width = newWidth;
            _description.ArraySize = newArraySize;
            _description.MipLevels = newMipMapCount;
            _description.Format = newFormat;
        }
    }
}

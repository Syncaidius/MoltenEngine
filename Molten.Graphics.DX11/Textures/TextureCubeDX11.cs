using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class TextureCubeDX11 : TextureBase, ITextureCube
    {
        internal ID3D11Texture2D* NativeTexture;
        Texture2DDesc _description;

        internal TextureCubeDX11(RendererDX11 renderer, uint width,
            uint height, Format format = Format.FormatR8G8B8A8Unorm, uint mipCount = 1, 
            uint cubeCount = 1, TextureFlags flags = TextureFlags.None)
            : base(renderer, width, height, 1, mipCount, 6, AntiAliasLevel.None, MSAAQuality.Default, format, flags)
        {
            CubeCount = cubeCount;
            _description = new Texture2DDesc()
            {
                Width = width,
                Height = height,
                MipLevels = mipCount,
                ArraySize = 6 * CubeCount,
                Format = format,
                BindFlags = (uint)BindFlag.BindShaderResource,
                CPUAccessFlags = (uint)GetAccessFlags(),
                SampleDesc = new SampleDesc()
                {
                    Count = 1,
                    Quality = 0,
                },
                Usage = GetUsageFlags(),
                MiscFlags = (uint)(GetResourceFlags() | ResourceMiscFlag.ResourceMiscTexturecube),
            };
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
        {
            desc.Format = DxgiFormat;
            desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexturecubearray;
            desc.TextureCubeArray = new TexcubeArraySrv()
            {
                MostDetailedMip = 0,
                MipLevels = _description.MipLevels,
                NumCubes = CubeCount,
                First2DArrayFace = 0,
            };
        }

        protected override void SetUAVDescription(ref ShaderResourceViewDesc srvDesc, ref UnorderedAccessViewDesc desc)
        {
            desc.Format = srvDesc.Format;
            desc.ViewDimension = UavDimension.UavDimensionTexture2Darray;

            desc.Texture2DArray = new Tex2DArrayUav()
            {
                ArraySize = _description.ArraySize,
                FirstArraySlice = srvDesc.Texture2DArray.FirstArraySlice,
                MipSlice = 0,
            };

            desc.Buffer = new BufferUav()
            {
                FirstElement = 0,
                NumElements = _description.Width * _description.Height * _description.ArraySize,
            };
        }

        protected override ID3D11Resource* CreateResource(bool resize)
        {
            SubresourceData* subData = null;
            Device.NativeDevice->CreateTexture2D(ref _description, subData, ref NativeTexture);
            return (ID3D11Resource*)NativeTexture;
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            _description.Width = newWidth;
            _description.Height = newHeight;
            _description.MipLevels = newMipMapCount;
            _description.Format = newFormat;
        }

        public void Resize(uint newWidth, uint newHeight, uint newMipMapCount)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = newMipMapCount,
            });
        }

        public void Resize(uint newWidth, uint newMipMapCount)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = Height,
                NewMipMapCount = newMipMapCount,
            });
        }

        /// <summary>Gets information about the texture.</summary>
        internal ref Texture2DDesc Description => ref _description;

        /// <summary>Gets the number of cube maps stored in the texture. This is greater than 1 if the texture is a cube-map array.</summary>
        public uint CubeCount { get; private set; }
    }
}

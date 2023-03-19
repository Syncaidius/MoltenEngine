using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class TextureCubeDX11 : TextureDX11, ITextureCube
    {
        internal ID3D11Texture2D1* NativeTexture;
        Texture2DDesc1 _desc;

        internal TextureCubeDX11(RenderService renderer, uint width,
            uint height, Format format = Format.FormatR8G8B8A8Unorm, uint mipCount = 1, 
            uint cubeCount = 1, TextureFlags flags = TextureFlags.None, string name = null)
            : base(renderer, width, height, 1, mipCount, 6, AntiAliasLevel.None, MSAAQuality.Default, format, flags, name)
        {
            CubeCount = cubeCount;
            _desc = new Texture2DDesc1()
            {
                Width = width,
                Height = height,
                MipLevels = mipCount,
                ArraySize = 6 * CubeCount,
                Format = format,
                BindFlags = (uint)BindFlag.ShaderResource,
                CPUAccessFlags = (uint)GetAccessFlags(),
                SampleDesc = new SampleDesc()
                {
                    Count = 1,
                    Quality = 0,
                },
                Usage = GetUsageFlags(),
                MiscFlags = (uint)(GetResourceFlags() | ResourceMiscFlag.Texturecube),
                TextureLayout = TextureLayout.None
            };
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
        {
            desc.Format = DxgiFormat;
            desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexturecubearray;
            desc.TextureCubeArray = new TexcubeArraySrv()
            {
                MostDetailedMip = 0,
                MipLevels = _desc.MipLevels,
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
                ArraySize = _desc.ArraySize,
                FirstArraySlice = srvDesc.Texture2DArray.FirstArraySlice,
                MipSlice = 0,
                PlaneSlice = 0
            };

            desc.Buffer = new BufferUav()
            {
                FirstElement = 0,
                NumElements = _desc.Width * _desc.Height * _desc.ArraySize,
            };
        }

        protected override ID3D11Resource* CreateResource(bool resize)
        {
            SubresourceData* subData = null;
            (Device as DeviceDX11).Ptr->CreateTexture2D1(ref _desc, subData, ref NativeTexture);
            return (ID3D11Resource*)NativeTexture;
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            _desc.Width = newWidth;
            _desc.Height = newHeight;
            _desc.MipLevels = newMipMapCount;
            _desc.Format = newFormat;
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
        internal ref Texture2DDesc1 Description => ref _desc;

        /// <summary>Gets the number of cube maps stored in the texture. This is greater than 1 if the texture is a cube-map array.</summary>
        public uint CubeCount { get; private set; }

        internal override Usage UsageFlags => _desc.Usage;

        public override bool IsUnorderedAccess => ((BindFlag)_desc.BindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess;
    }
}

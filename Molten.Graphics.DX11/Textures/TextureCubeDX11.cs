using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Silk.NET.DXGI;
using Silk.NET.Direct3D11;
using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public unsafe class TextureCubeDX11 : TextureBase, ITextureCube
    {
        internal ID3D11Texture2D* NativeTexture;
        Texture2DDesc _description;

        public event TextureHandler OnPreResize;
        public event TextureHandler OnPostResize;

        internal TextureCubeDX11(RendererDX11 renderer, int width,
            int height, Format format = Format.FormatR8G8B8A8Unorm, int mipCount = 1, 
            int cubeCount = 1, TextureFlags flags = TextureFlags.None)
            : base(renderer, width, height, 1, mipCount, 6, 1, format, flags)
        {
            CubeCount = cubeCount;
            _description = new Texture2DDesc()
            {
                Width = (uint)width,
                Height = (uint)height,
                MipLevels = (uint)mipCount,
                ArraySize = (uint)(6 * CubeCount),
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
                NumCubes = (uint)CubeCount,
                First2DArrayFace = 0,
            };
        }

        protected override void SetUAVDescription(ref ShaderResourceViewDesc srvDesc, ref UnorderedAccessViewDesc desc)
        {
            desc.Format = SRV.Description.Format;
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
            Device.Native->CreateTexture2D(ref _description, subData, ref NativeTexture);
            return (ID3D11Resource*)NativeTexture;
        }

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
        {
            _description.Width = (uint)newWidth;
            _description.Height = (uint)newHeight;
            _description.MipLevels = (uint)newMipMapCount;
            _description.Format = newFormat;
        }

        public void Resize(int newWidth, int newHeight, int newMipMapCount)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = newMipMapCount,
            });
        }

        public void Resize(int newWidth, int newMipMapCount)
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
        public int CubeCount { get; private set; }
    }
}

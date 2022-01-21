using System;
using Silk.NET.DXGI;
using System.IO;
using Silk.NET.Direct3D11;
using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public unsafe class Texture2DDX11 : TextureBase, ITexture2D
    {
        internal ID3D11Texture2D* NativeTexture;
        protected Texture2DDesc _description;

        /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        /// <param name="flags">A set of flags to override those of the provided texture.</param>
        internal Texture2DDX11(Texture2DDX11 descTexture, TextureFlags flags)
            : this(descTexture.Renderer as RendererDX11, descTexture.Width, descTexture.Height, 
                  descTexture.DxgiFormat, descTexture.MipMapCount, descTexture.ArraySize, flags)
        { }

        /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        internal Texture2DDX11(Texture2DDX11 descTexture)
            : this(descTexture.Renderer as RendererDX11, descTexture.Width, descTexture.Height, descTexture.DxgiFormat, descTexture.MipMapCount, descTexture.ArraySize, descTexture.Flags)
        { }

        internal Texture2DDX11(
            RendererDX11 renderer,
            int width,
            int height,
            Format format = Format.FormatR8G8B8A8Unorm,
            int mipCount = 1,
            int arraySize = 1,
            TextureFlags flags = TextureFlags.None,
            int sampleCount = 1)
            : base(renderer, width, height, 1, mipCount, arraySize, sampleCount, format, flags)
        {
            _description = new Texture2DDesc()
            {
                Width = (uint)Math.Max(width, 1),
                Height = (uint)Math.Max(height, 1),
                MipLevels = (uint)mipCount,
                ArraySize = (uint)Math.Max(arraySize, 1),
                Format = format,
                BindFlags = (uint)GetBindFlags(),
                CPUAccessFlags = (uint)GetAccessFlags(),
                SampleDesc = new SampleDesc()
                {
                    Count = (uint)Math.Max(sampleCount, 1),
                    Quality = 0,
                },
                Usage = GetUsageFlags(),
                MiscFlags = (uint)GetResourceFlags(),
            };
        }

        public Texture2DProperties Get2DProperties()
        {
            return new Texture2DProperties()
            {
                Width = Width,
                Height = Height,
                ArraySize = ArraySize,
                Flags = Flags,
                Format = this.DataFormat,
                MipMapLevels = MipMapCount,
                SampleCount = SampleCount,
            };
        }

        protected override unsafe ID3D11Resource* CreateResource(bool resize)
        {
            SubresourceData* subData = null;
            Device.Native->CreateTexture2D(ref _description, subData, ref NativeTexture);
            return (ID3D11Resource*)NativeTexture;
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
        {
            if (_description.SampleDesc.Count > 1)
            {
                desc.ViewDimension = D3DSrvDimension.D3D101SrvDimensionTexture2Dmsarray;
                desc.Texture2DMSArray = new Tex2DmsArraySrv()
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2Darray;
                desc.Texture2DArray = new Tex2DArraySrv()
                {
                    ArraySize = _description.ArraySize,
                    MipLevels = _description.MipLevels,
                    MostDetailedMip = 0,
                    FirstArraySlice = 0,
                };
            }
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

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, 
            int newMipMapCount, int newArraySize, Format newFormat)
        {
            _description.ArraySize = (uint)newArraySize;
            _description.Width = (uint)newWidth;
            _description.Height = (uint)newHeight;
            _description.MipLevels = (uint)newMipMapCount;
            _description.Format = newFormat;
        }

        public void Resize(int newWidth, int newHeight)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = MipMapCount,
                NewArraySize = (int)_description.ArraySize,
                NewFormat = DxgiFormat
            });
        }

        public void Resize(int newWidth, int newHeight, int newMipMapCount)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = newMipMapCount,
                NewArraySize = (int)_description.ArraySize,
                NewFormat = DxgiFormat
            });
        }

        public void Resize(int newWidth, int newHeight, int newMipMapCount, int newArraySize, GraphicsFormat newFormat)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = newMipMapCount,
                NewArraySize = newArraySize,
                NewFormat = newFormat.ToApi(),
            });
        }
    }
}

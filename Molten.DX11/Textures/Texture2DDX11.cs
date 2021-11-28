using Molten.Graphics.Textures.DDS;
using Molten.Graphics.Textures;
using System;

namespace Molten.Graphics
{
    using Silk.NET.DXGI;
    using System.IO;
    using System.Runtime.InteropServices;
    using Resource = SharpDX.Direct3D11.Resource;

    public class Texture2DDX11 : TextureBase, ITexture2D
    {
        protected Texture2D _texture;
        protected Texture2DDescription _description;

        /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        /// <param name="flags">A set of flags to override those of the provided texture.</param>
        internal Texture2DDX11(Texture2DDX11 descTexture, TextureFlags flags)
            : this(descTexture.Renderer as RendererDX11, descTexture.Width, descTexture.Height, 
                  descTexture.DxFormat, descTexture.MipMapCount, descTexture.ArraySize, flags)
        { }

        /// <summary>Creates a new instance of <see cref="Texture2DDX11"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        internal Texture2DDX11(Texture2DDX11 descTexture)
            : this(descTexture.Renderer as RendererDX11, descTexture.Width, descTexture.Height, descTexture.DxFormat, descTexture.MipMapCount, descTexture.ArraySize, descTexture.Flags)
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
            _description = new Texture2DDescription()
            {
                Width = Math.Max(width, 1),
                Height = Math.Max(height, 1),
                MipLevels = mipCount,
                ArraySize = Math.Max(arraySize, 1),
                Format = format,
                BindFlags = GetBindFlags(),
                CpuAccessFlags = GetAccessFlags(),
                SampleDescription = new SampleDescription()
                {
                    Count = Math.Max(sampleCount, 1),
                    Quality = 0,
                },
                Usage = GetUsageFlags(),
                OptionFlags = GetResourceFlags(),
            };
        }

        public Texture2DProperties Get2DProperties()
        {
            return new Texture2DProperties()
            {
                Width = _width,
                Height = _height,
                ArraySize = _arraySize,
                Flags = _flags,
                Format = this.DataFormat,
                MipMapLevels = _mipCount,
                SampleCount = _sampleCount,
            };
        }

        protected override Resource CreateResource(bool resize)
        {
            _texture = new Texture2D(Device.D3d, _description);
            return _texture;
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDescription desc)
        {
            if (_description.SampleDescription.Count > 1)
            {
                desc.Dimension = ShaderResourceViewDimension.Texture2DMultisampledArray;
                desc.Texture2DMSArray = new ShaderResourceViewDescription.Texture2DMultisampledArrayResource()
                {
                    ArraySize = _description.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                desc.Dimension = ShaderResourceViewDimension.Texture2DArray;
                desc.Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                {
                    ArraySize = _description.ArraySize,
                    MipLevels = _description.MipLevels,
                    MostDetailedMip = 0,
                    FirstArraySlice = 0,
                };
            }
        }

        protected override void SetUAVDescription(ShaderResourceViewDescription srvDesc, ref UnorderedAccessViewDescription desc)
        {
            desc.Format = SRV.Description.Format;
            desc.Dimension = UnorderedAccessViewDimension.Texture2DArray;

            desc.Texture2DArray = new UnorderedAccessViewDescription.Texture2DArrayResource()
            {
                ArraySize = _description.ArraySize,
                FirstArraySlice = srvDesc.Texture2DArray.FirstArraySlice,
                MipSlice = 0,
            };

            desc.Buffer = new UnorderedAccessViewDescription.BufferResource()
            {
                FirstElement = 0,
                ElementCount = _description.Width * _description.Height * _description.ArraySize,
            };
        }

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, 
            int newMipMapCount, int newArraySize, Format newFormat)
        {
            _description.ArraySize = newArraySize;
            _description.Width = newWidth;
            _description.Height = newHeight;
            _description.MipLevels = newMipMapCount;
            _description.Format = newFormat;
        }

        public void Resize(int newWidth, int newHeight)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = _mipCount,
                NewArraySize = _description.ArraySize,
                NewFormat = _format
            });
        }

        public void Resize(int newWidth, int newHeight, int newMipMapCount)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = newMipMapCount,
                NewArraySize = _description.ArraySize,
                NewFormat = _format
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

        /// <summary>Gets the underlying DirectX Texture2D object.</summary>
        internal Texture2D TextureResource => _texture;
    }
}

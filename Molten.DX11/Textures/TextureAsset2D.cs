using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Molten.Graphics.Textures.DDS;
using Molten.Graphics.Textures;
using System;

namespace Molten.Graphics
{
    using SharpDX.Direct3D;
    using System.IO;
    using System.Runtime.InteropServices;
    using Resource = SharpDX.Direct3D11.Resource;

    public class TextureAsset2D : TextureBase, ITexture2D
    {
        protected Texture2D _texture;
        protected Texture2DDescription _description;

        public event TextureHandler OnPreResize;
        public event TextureHandler OnPostResize;

        /// <summary>Creates a new instance of <see cref="TextureAsset2D"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        /// <param name="flags">A set of flags to override those of the provided texture.</param>
        internal TextureAsset2D(TextureAsset2D descTexture, TextureFlags flags)
            : this(descTexture.Device, descTexture.Width, descTexture.Height, descTexture.DxFormat, descTexture.MipMapLevels, descTexture.ArraySize, flags)
        { }

        /// <summary>Creates a new instance of <see cref="TextureAsset2D"/> and uses a provided texture for its description. Note: This does not copy the contents 
        /// of the provided texture in to the new instance.</summary>
        /// <param name="descTexture"></param>
        internal TextureAsset2D(TextureAsset2D descTexture)
            : this(descTexture.Device, descTexture.Width, descTexture.Height, descTexture.DxFormat, descTexture.MipMapLevels, descTexture.ArraySize, descTexture.Flags)
        { }

        internal TextureAsset2D(
            GraphicsDeviceDX11 device,
            int width,
            int height,
            Format format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
            int mipCount = 1,
            int arraySize = 1,
            TextureFlags flags = TextureFlags.None,
            int sampleCount = 1)
            : base(device, width, height, 1, mipCount, arraySize, sampleCount, format, flags)
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

            _resourceViewDescription = new ShaderResourceViewDescription();
            _resourceViewDescription.Format = _format;

            if (_description.ArraySize > 1)
            {
                if (sampleCount > 1)
                {
                    _resourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2DMultisampledArray;
                    _resourceViewDescription.Texture2DMSArray = new ShaderResourceViewDescription.Texture2DMultisampledArrayResource()
                    {
                        ArraySize = arraySize,
                        FirstArraySlice = 0,
                    };
                }
                else
                {
                    _resourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2DArray;
                    _resourceViewDescription.Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                    {
                        ArraySize = _description.ArraySize,
                        MipLevels = _description.MipLevels,
                        MostDetailedMip = 0,
                        FirstArraySlice = 0,
                    };
                }
            }
            else
            {
                if (sampleCount > 1)
                {
                    _resourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2DMultisampled;
                    _resourceViewDescription.Texture2DMS = new ShaderResourceViewDescription.Texture2DMultisampledResource();
                }
                else
                {
                    _resourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2D;
                    _resourceViewDescription.Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                    {
                        MipLevels = _description.MipLevels,
                        MostDetailedMip = 0,
                    };
                }
            }
        }

        protected override Resource CreateTextureInternal(bool resize)
        {
            _description.Width = _width;
            _description.Height = _height;
            _texture = new Texture2D(Device.D3d, _description);
            return _texture;
        }

        protected override void OnCreateUAV()
        {
            DisposeObject(ref _uav);

            UnorderedAccessViewDescription uDesc;

            if (_description.ArraySize > 1)
            {
                uDesc = new UnorderedAccessViewDescription()
                {
                    Format = SRV.Description.Format,
                    Dimension = UnorderedAccessViewDimension.Texture2DArray,
                    Texture2DArray = new UnorderedAccessViewDescription.Texture2DArrayResource()
                    {
                        ArraySize = _description.ArraySize,
                        FirstArraySlice = _resourceViewDescription.Texture2DArray.FirstArraySlice,
                        MipSlice = 0,
                    },

                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        FirstElement = 0,
                        ElementCount = _description.Width * _description.Height * _description.ArraySize,
                    }
                };
            }
            else
            {
                uDesc = new UnorderedAccessViewDescription()
                {
                    Format = SRV.Description.Format,
                    /*Texture2D = new UnorderedAccessViewDescription.Texture2DResource(){
                        MipSlice = 0,
                    },*/
                    Dimension = UnorderedAccessViewDimension.Texture2D,
                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        FirstElement = 0,
                        ElementCount = _description.Width * _description.Height,
                    }
                };
            }

            UAV = new UnorderedAccessView(Device.D3d, _texture, uDesc);
        }

        protected override void BeforeResize()
        {
            OnPreResize?.Invoke(this);
        }

        protected override void AfterResize()
        {
            OnPostResize?.Invoke(this);
        }

        protected override void OnSetSize(int newWidth, int newHeight, int newDepth, int newArraySize)
        {
            _description.ArraySize = newArraySize;
            _description.Width = newWidth;
            _description.Height = newHeight;
        }

        public void Resize(int newWidth, int newHeight)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewArraySize = _description.ArraySize,
            });
        }

        public void Resize(int newWidth)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = _height,
                NewArraySize = _description.ArraySize,
            });
        }

        public void Resize(int newWidth, int newHeight, int newArraySize)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewArraySize = newArraySize,
            });
        }

        /// <summary>Gets the underlying DirectX Texture2D object.</summary>
        internal Texture2D TextureResource
        {
            get { return _texture; }
        }

        /// <summary>Gets information about the texture.</summary>
        internal Texture2DDescription Description { get { return _description; } }
    }
}

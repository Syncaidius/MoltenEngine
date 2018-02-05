using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Molten.Graphics.Textures.DDS;
using Molten.Graphics.Textures;
using Molten.Utilities;
using System;

namespace Molten.Graphics
{
    using SharpDX.Direct3D;
    using System.IO;
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

        internal TextureAsset2D(GraphicsDevice device, int width,
            int height, Format format = SharpDX.DXGI.Format.R8G8B8A8_UNorm, int mipCount = 1, int arraySize = 1, TextureFlags flags = TextureFlags.None)
            : base(device, width, height, 1, mipCount, format, flags)
        {
            _description = new Texture2DDescription()
            {
                Width = Math.Max(1, width),
                Height = Math.Max(1, height),
                MipLevels = mipCount, // Setting to 0 will generate sub-textures.
                ArraySize = Math.Max(arraySize, 1),
                Format = format,
                BindFlags = GetBindFlags(),
                CpuAccessFlags = GetAccessFlags(),
                SampleDescription = new SampleDescription()
                {
                    Count = 1,
                    Quality = 0,
                },
                Usage = GetUsageFlags(),
                OptionFlags = GetResourceFlags(),
            };

            if (_description.ArraySize > 1)
            {
                _resourceViewDescription = new ShaderResourceViewDescription()
                {
                    Format = _format,
                    Dimension = ShaderResourceViewDimension.Texture2DArray,
                    Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                    {
                        ArraySize = _description.ArraySize,
                        MipLevels = _description.MipLevels,
                        MostDetailedMip = 0,
                        FirstArraySlice = 0,
                    },
                };
            }
            else
            {
                _resourceViewDescription = new ShaderResourceViewDescription()
                {
                    Format = _format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                    {
                        MipLevels = _description.MipLevels,
                        MostDetailedMip = 0,
                    },
                };
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

        /// <summary>Maps the texture resource via a staging texture, which is passed into the provided callback to be used however you want.
        /// Note: </summary>
        /// <param name="callback">The callback to invoke once the texture is mapped for CPU access.</param>
        internal void Map(GraphicsPipe pipe, Action<GraphicsDevice, Texture2D> callback)
        {
            //create a temporary staging texture
            Texture2DDescription stageDesc = _description;
            stageDesc.CpuAccessFlags = CpuAccessFlags.Read;
            stageDesc.Usage = ResourceUsage.Staging;
            stageDesc.BindFlags = BindFlags.None;
            Texture2D staging = new Texture2D(Device.D3d, stageDesc);

            //copy the texture into the staging texture.
            pipe.Context.CopyResource(_texture, staging);

            //invoke the callback
            callback(Device, staging);

            //dispose of staging texture
            staging.Dispose();
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

        /// <summary>Gets whether or not the texture is a texture array. If the array size is greater than 1, it is considered a texture array.</summary>
        public override bool IsTextureArray { get { return _description.ArraySize > 1; } }

        /// <summary>Gets the maximum number of array slices in the texture resource.</summary>
        public override int ArraySize { get { return _description.ArraySize; } }

        /// <summary>Gets the number of mip-maps in the texture.</summary>
        public override int MipMapLevels { get { return _description.MipLevels; } }
    }
}

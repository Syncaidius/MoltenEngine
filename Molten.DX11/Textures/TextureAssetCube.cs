using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Molten.Graphics.Textures.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class TextureAssetCube : TextureBase, ITextureCube
    {
        protected Texture2D _texture;
        protected Texture2DDescription _description;

        public event TextureHandler OnPreResize;
        public event TextureHandler OnPostResize;

        internal TextureAssetCube(GraphicsDevice device, int width,
            int height, Format format = SharpDX.DXGI.Format.R8G8B8A8_UNorm, int mipCount = 1, TextureFlags flags = TextureFlags.None)
            : base(device, width, height, 1, mipCount, format, flags)
        {
            _description = new Texture2DDescription()
            {
                Width = width,
                Height = height,
                MipLevels = mipCount,
                ArraySize = 6,
                Format = format,
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                CpuAccessFlags = GetAccessFlags(),
                SampleDescription = new SampleDescription()
                {
                    Count = 1,
                    Quality = 0,
                },
                Usage = GetUsageFlags(),
                OptionFlags = GetResourceFlags() | SharpDX.Direct3D11.ResourceOptionFlags.TextureCube,
            };

            _resourceViewDescription.Format = _format;
            _resourceViewDescription.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
            _resourceViewDescription.TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
            {
                MostDetailedMip = 0,
                MipLevels = _description.MipLevels,
            };
        }

        protected override SharpDX.Direct3D11.Resource CreateTextureInternal(bool resize)
        {
            _texture = new Texture2D(Device.D3d, _description);
            return _texture;
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
            _description.Width = newWidth;
            _description.Height = newHeight;
        }

        public void Resize(int newWidth, int newHeight)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
            });
        }

        public void Resize(int newWidth)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = newWidth,
                NewHeight = _height,
            });
        }

        /// <summary>Gets the underlying DirectX Texture2D object.</summary>
        public Texture2D TextureResource
        {
            get { return _texture; }
        }

        /// <summary>Gets information about the texture.</summary>
        public Texture2DDescription Description { get { return _description; } }

        /// <summary>Gets the number of array slices in the texture. For a texture cube, this is always 6.</summary>
        public override int ArraySize { get { return _description.ArraySize; } }

        /// <summary>Gets whether or not the texture is a texture array. This is always true for texture cubes.</summary>
        public override bool IsTextureArray { get { return true; } }

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        public override int MipMapLevels { get { return _description.MipLevels; } }
    }
}

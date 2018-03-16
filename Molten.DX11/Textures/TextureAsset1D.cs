using SharpDX.Direct3D;
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
    public class TextureAsset1D : TextureBase, ITexture
    {
        protected Texture1D _texture;
        protected Texture1DDescription _description;

        public event TextureHandler OnPreResize;

        public event TextureHandler OnPostResize;

        internal TextureAsset1D(
            GraphicsDevice device, 
            int width, 
            Format format = SharpDX.DXGI.Format.R8G8B8A8_UNorm, 
            int mipCount = 1, 
            int arraySize = 1,
            TextureFlags flags = TextureFlags.None)
            : base(device, width, 1, 1, mipCount, arraySize, 1, format, flags)
        {
            if (_isBlockCompressed)
                throw new NotSupportedException("1D textures do not supports block-compressed formats.");

            arraySize = Math.Max(0, arraySize); // Minimum array size of 1.

            _description = new Texture1DDescription()
            {
                Width = width,
                MipLevels = mipCount,
                ArraySize = arraySize,
                Format = format,
                BindFlags = GetBindFlags(),
                CpuAccessFlags = GetAccessFlags(),
                Usage = GetUsageFlags(),
                OptionFlags = GetResourceFlags(),
            };

            // Setup SRV description
            if (arraySize > 1)
            {
                _resourceViewDescription = new ShaderResourceViewDescription()
                {
                    Format = _format,
                    Dimension = ShaderResourceViewDimension.Texture1DArray,
                    Texture1DArray = new ShaderResourceViewDescription.Texture1DArrayResource()
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
                    Dimension = ShaderResourceViewDimension.Texture1D,
                    Texture1D = new ShaderResourceViewDescription.Texture1DResource()
                    {
                        MipLevels = _description.MipLevels,
                        MostDetailedMip = 0,
                    },
                };
            }
        }

        protected override SharpDX.Direct3D11.Resource CreateTextureInternal(bool resize)
        {
            _texture = new Texture1D(Device.D3d, _description);

            return _texture;
        }

        protected override void OnCreateUAV()
        {
            UAV?.Dispose();
            UAV = null;

            UnorderedAccessViewDescription uDesc;

            if (_description.ArraySize > 1)
            {
                uDesc = new UnorderedAccessViewDescription()
                {
                    Format = SRV.Description.Format,
                    Dimension = UnorderedAccessViewDimension.Texture1DArray,
                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        FirstElement = 0,
                        ElementCount = _description.Width * _description.ArraySize,
                    }
                };
            }
            else
            {
                uDesc = new UnorderedAccessViewDescription()
                {
                    Format = SRV.Description.Format,
                    Dimension = UnorderedAccessViewDimension.Texture1D,
                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        FirstElement = 0,
                        ElementCount = _description.Width,
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
            _description.Width = newWidth;
            _description.ArraySize = newArraySize;
        }

        public void Resize(int newWidth)
        {
            QueueChange(new TextureResize()
            {
                NewWidth = 1,
            });
        }
    }
}

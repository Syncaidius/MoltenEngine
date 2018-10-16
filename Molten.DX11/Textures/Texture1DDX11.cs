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
    public class Texture1DDX11 : TextureBase, ITexture
    {
        protected Texture1D _texture;
        protected Texture1DDescription _description;

        public event TextureHandler OnPreResize;

        public event TextureHandler OnPostResize;

        internal Texture1DDX11(
            RendererDX11 renderer, 
            int width, 
            Format format = SharpDX.DXGI.Format.R8G8B8A8_UNorm, 
            int mipCount = 1, 
            int arraySize = 1,
            TextureFlags flags = TextureFlags.None)
            : base(renderer, width, 1, 1, mipCount, arraySize, 1, format, flags)
        {
            if (_isBlockCompressed)
                throw new NotSupportedException("1D textures do not supports block-compressed formats.");

            _description = new Texture1DDescription()
            {
                Width = width,
                MipLevels = mipCount,
                ArraySize = Math.Max(1, arraySize),
                Format = format,
                BindFlags = GetBindFlags(),
                CpuAccessFlags = GetAccessFlags(),
                Usage = GetUsageFlags(),
                OptionFlags = GetResourceFlags(),
            };
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDescription desc)
        {
            desc.Format = _format;
            desc.Dimension = ShaderResourceViewDimension.Texture1DArray;
            desc.Texture1DArray = new ShaderResourceViewDescription.Texture1DArrayResource()
            {
                ArraySize = _description.ArraySize,
                MipLevels = _description.MipLevels,
                MostDetailedMip = 0,
                FirstArraySlice = 0,
            };
        }

        protected override void SetUAVDescription(ShaderResourceViewDescription srvDesc, ref UnorderedAccessViewDescription desc)
        {
            desc.Format = SRV.Description.Format;
            desc.Dimension = UnorderedAccessViewDimension.Texture1DArray;
            desc.Buffer = new UnorderedAccessViewDescription.BufferResource()
            {
                FirstElement = 0,
                ElementCount = _description.Width * _description.ArraySize,
            };
        }

        protected override SharpDX.Direct3D11.Resource CreateResource(bool resize)
        {
            _texture = new Texture1D(Device.D3d, _description);
            return _texture;
        }

        protected override void UpdateDescription(int newWidth, int newHeight, int newDepth, int newMipMapCount, int newArraySize, Format newFormat)
        {
            _description.Width = newWidth;
            _description.ArraySize = newArraySize;
            _description.MipLevels = newMipMapCount;
            _description.Format = newFormat;
        }
    }
}

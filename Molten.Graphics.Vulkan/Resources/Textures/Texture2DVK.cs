using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public class Texture2DVK : TextureVK, ITexture2D
    {
        public Texture2DVK(GraphicsDevice device, GraphicsTextureType type, TextureDimensions dimensions, 
            AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GraphicsFormat format, 
            GraphicsResourceFlags flags, bool allowMipMapGen, string name) :
            base(device, type, dimensions, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
        }

        protected override void OnGenerateMipMaps(GraphicsQueue cmd)
        {
            throw new NotImplementedException();
        }

        protected override void SetCreateInfo(ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            throw new NotImplementedException();
        }
    }
}

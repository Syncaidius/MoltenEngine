using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public class Texture3DVK : TextureVK, ITexture3D
    {
        public Texture3DVK(GraphicsDevice device, GraphicsTextureType type, 
            TextureDimensions dimensions, AntiAliasLevel aaLevel, 
            MSAAQuality sampleQuality, GraphicsFormat format, 
            GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, type, dimensions, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
        }

        protected override void SetCreateInfo(ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            imgInfo.ImageType = ImageType.Type3D;
        }
    }
}

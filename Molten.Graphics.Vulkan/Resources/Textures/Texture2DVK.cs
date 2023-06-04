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
        internal Texture2DVK(GraphicsDevice device, GraphicsTextureType type, TextureDimensions dimensions,
            AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GraphicsFormat format,
            GraphicsResourceFlags flags, bool allowMipMapGen, string name) :
            base(device, 
                type, 
                dimensions, 
                aaLevel, 
                sampleQuality, 
                format, flags, 
                allowMipMapGen,
                name)
        {
            // Validate that only a 2D texture type was provided.
            switch (type)
            {
                default:
                    throw new NotSupportedException("The specified texture type is not a 2D texture type.");

                case GraphicsTextureType.Texture2D:
                case GraphicsTextureType.Surface2D:
                case GraphicsTextureType.TextureCube:
                    return;
            }
        }

        protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            imgInfo.ImageType = ImageType.Type2D;
            viewInfo.ViewType = ArraySize == 1 ? ImageViewType.Type2D : ImageViewType.Type2DArray;
        }
    }
}

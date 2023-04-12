using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public class TextureCubeVK : Texture2DVK, ITextureCube
    {
        public TextureCubeVK(GraphicsDevice device, GraphicsTextureType type, TextureDimensions dimensions, 
            AntiAliasLevel aaLevel, MSAAQuality sampleQuality, 
            GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, type, dimensions, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
        }

        protected override void SetCreateInfo(ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            base.SetCreateInfo(ref imgInfo, ref  viewInfo);

            imgInfo.Flags |= ImageCreateFlags.CreateCubeCompatibleBit;
            imgInfo.ArrayLayers *= 6;
        }

        public uint CubeCount { get; }
    }
}

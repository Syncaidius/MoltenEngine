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
        public TextureCubeVK(GraphicsDevice device, uint cubeCount, TextureDimensions dimensions, 
            GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, GraphicsTextureType.TextureCube, dimensions, AntiAliasLevel.None, MSAAQuality.Default, format, flags, allowMipMapGen, name)
        {
            CubeCount = cubeCount;
        }

        protected override void SetCreateInfo(ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            base.SetCreateInfo(ref imgInfo, ref  viewInfo);

            imgInfo.Flags |= ImageCreateFlags.CreateCubeCompatibleBit;
            imgInfo.ArrayLayers *= 6;

            viewInfo.ViewType = CubeCount == 1 ? ImageViewType.TypeCube : ImageViewType.TypeCubeArray;
        }

        public uint CubeCount { get; }
    }
}

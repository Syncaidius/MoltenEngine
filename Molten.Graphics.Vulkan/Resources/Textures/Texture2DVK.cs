using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public class Texture2DVK : TextureVK, ITexture2D
    {
        internal Texture2DVK(DeviceVK device, GraphicsTextureType type, uint width, uint height, uint mipCount, uint arraySize,
            AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GraphicsFormat format,
            GraphicsResourceFlags flags, bool allowMipMapGen, string name) :
            base(device, 
                type, 
                new TextureDimensions(width, height, 1 , mipCount, arraySize), 
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

        public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newMipMapCount = 0, 
            uint newArraySize = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
        {
            Resize(priority, newWidth, newHeight, newArraySize, newMipMapCount, Depth, newFormat);
        }

        protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            imgInfo.ImageType = ImageType.Type2D;
            viewInfo.ViewType = ArraySize == 1 ? ImageViewType.Type2D : ImageViewType.Type2DArray;
        }
    }
}

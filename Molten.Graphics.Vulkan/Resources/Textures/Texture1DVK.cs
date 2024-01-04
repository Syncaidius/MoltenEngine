using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

public class Texture1DVK : TextureVK, ITexture1D
{
    internal Texture1DVK(DeviceVK device, uint width, uint mipCount, uint arraySize, 
        GraphicsFormat format, GraphicsResourceFlags flags, string name) : 
        base(device, new TextureDimensions(width, 1, 1, mipCount, arraySize), AntiAliasLevel.None, MSAAQuality.Default, format,flags, name)
    {

    }

    protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
    {
        imgInfo.ImageType = ImageType.Type1D;
        viewInfo.ViewType = ArraySize == 1 ? ImageViewType.Type1D : ImageViewType.Type1DArray;
    }
}

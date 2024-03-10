using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

public class Texture1DVK : TextureVK, ITexture1D
{
    internal Texture1DVK(DeviceVK device, uint width, uint mipCount, uint arraySize, 
        GpuResourceFormat format, GpuResourceFlags flags, string name) : 
        base(device, new TextureDimensions(width, 1, 1, mipCount, arraySize), format,flags, name)
    {

    }

    protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
    {
        imgInfo.ImageType = ImageType.Type1D;
        viewInfo.ViewType = ArraySize == 1 ? ImageViewType.Type1D : ImageViewType.Type1DArray;
    }
}

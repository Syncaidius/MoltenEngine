using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

public class Texture2DVK : TextureVK, ITexture2D
{
    internal Texture2DVK(DeviceVK device, uint width, uint height, uint mipCount, uint arraySize,
        AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GpuResourceFormat format,
        GpuResourceFlags flags, string name) :
        base(device,
            new TextureDimensions(width, height, 1, mipCount, arraySize, aaLevel, sampleQuality), format, flags, name)
    { }

    public void Resize(GpuPriority priority, uint newWidth, uint newHeight, uint newMipMapCount = 0,
        uint newArraySize = 0, GpuResourceFormat newFormat = GpuResourceFormat.Unknown)
    {
        Resize(priority, newWidth, newHeight, newArraySize, newMipMapCount, Depth, newFormat);
    }

    protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
    {
        imgInfo.ImageType = ImageType.Type2D;
        viewInfo.ViewType = ArraySize == 1 ? ImageViewType.Type2D : ImageViewType.Type2DArray;
    }
}

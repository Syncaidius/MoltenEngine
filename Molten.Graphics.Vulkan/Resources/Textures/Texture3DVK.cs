using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

public class Texture3DVK : TextureVK, ITexture3D
{
    public Texture3DVK(DeviceVK device, 
        TextureDimensions dimensions, GraphicsFormat format, 
        GraphicsResourceFlags flags, string name, bool isSurface = false) : 
        base(device, 
            isSurface ? GraphicsTextureType.Texture3D : GraphicsTextureType.Surface3D, 
            dimensions, 
            AntiAliasLevel.None, 
            MSAAQuality.Default, 
            format, 
            flags, 
            name)
    {
    }

    public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
    {
        Resize(priority, newWidth, newHeight, ArraySize, newMipMapCount, newDepth, newFormat);
    }

    protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
    {
        imgInfo.ImageType = ImageType.Type3D;
        viewInfo.ViewType = ImageViewType.Type3D;
    }
}

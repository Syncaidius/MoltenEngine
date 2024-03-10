using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

public class TextureCubeVK : Texture2DVK, ITextureCube
{
    public TextureCubeVK(DeviceVK device, uint width, uint height, uint mipCount, uint arraySize, uint cubeCount, 
        GpuResourceFormat format, GpuResourceFlags flags, string name) : 
        base(device, width, height, mipCount, arraySize, 
            AntiAliasLevel.None, 
            MSAAQuality.Default, 
            format, 
            flags, 
            name)
    {
        CubeCount = cubeCount;
    }

    protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
    {
        base.SetCreateInfo(device, ref imgInfo, ref  viewInfo);

        imgInfo.Flags |= ImageCreateFlags.CreateCubeCompatibleBit;
        imgInfo.ArrayLayers *= 6;

        viewInfo.ViewType = CubeCount == 1 ? ImageViewType.TypeCube : ImageViewType.TypeCubeArray;
    }

    public uint CubeCount { get; }
}

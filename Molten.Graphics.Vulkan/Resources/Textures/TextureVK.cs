using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

public unsafe abstract class TextureVK : GpuTexture
{
    ImageCreateInfo _info;
    ImageViewCreateInfo _viewInfo;

    ResourceHandleVK<Image, ImageHandleVK> _handle;

    protected TextureVK(DeviceVK device, TextureDimensions dimensions, GpuResourceFormat format, GpuResourceFlags flags, string name) :
        base(device, ref dimensions, format, flags, name)
    {
        Device = device;
    }

    protected void SetHandle(ResourceHandleVK<Image, ImageHandleVK> handle)
    {
        _handle = handle;
    }

    protected override sealed void OnCreateResource()
    {
        // In Vulkan, the CPU either has read AND write access, or none at all.
        // If either of the CPU access flags were provided, we need to add both.
        if (Flags.Has(GpuResourceFlags.CpuRead) || Flags.Has(GpuResourceFlags.CpuWrite))
            Flags |= GpuResourceFlags.CpuRead | GpuResourceFlags.CpuWrite;


        ImageUsageFlags flags = ImageUsageFlags.None;
        if (Flags.Has(GpuResourceFlags.GpuRead))
            flags |= ImageUsageFlags.TransferSrcBit;

        if (Flags.Has(GpuResourceFlags.GpuWrite))
            flags |= ImageUsageFlags.TransferDstBit;

        if (Flags.Has(GpuResourceFlags.UnorderedAccess))
            flags |= ImageUsageFlags.StorageBit;

        if (!Flags.Has(GpuResourceFlags.DenyShaderAccess))
            flags |= ImageUsageFlags.SampledBit;

        _info = new ImageCreateInfo(StructureType.ImageCreateInfo);
        _info.Extent.Width = Width;
        _info.Extent.Height = Height;
        _info.Extent.Depth = Depth;
        _info.MipLevels = MipMapCount;
        _info.ArrayLayers = ArraySize;
        _info.Format = ResourceFormat.ToApi();
        _info.Tiling = ImageTiling.Optimal;
        _info.InitialLayout = ImageLayout.Undefined;
        _info.Usage = flags;
        _info.SharingMode = SharingMode.Exclusive;
        _info.Samples = SampleCountFlags.Count1Bit;
        _info.Flags = ImageCreateFlags.None;

        // Queue properties are ignored if sharing mode is not VK_SHARING_MODE_CONCURRENT.
        if (_info.SharingMode == SharingMode.Concurrent)
        {
            _info.PQueueFamilyIndices = EngineUtil.AllocArray<uint>(1);
            _info.PQueueFamilyIndices[0] = (Device.Queue as GraphicsQueueVK).Index;
            _info.QueueFamilyIndexCount = 1;
        }

        _viewInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo);
        _viewInfo.Format = _info.Format;
        _viewInfo.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
        _viewInfo.SubresourceRange.BaseMipLevel = 0;
        _viewInfo.SubresourceRange.LevelCount = MipMapCount;
        _viewInfo.SubresourceRange.BaseArrayLayer = 0;
        _viewInfo.SubresourceRange.LayerCount = ArraySize;
        _viewInfo.Flags = ImageViewCreateFlags.None;

        SetCreateInfo(Device, ref _info, ref _viewInfo);

        // Creation of images with tiling VK_IMAGE_TILING_LINEAR may not be supported unless other parameters meet all of the constraints
        if (_info.Tiling == ImageTiling.Linear)
        {
            //if (this is DepthSurfaceVK depthSurface)
            //    throw new GraphicsResourceException(this, "A depth surface texture cannot use linear tiling mode");

            if (_info.ImageType != ImageType.Type2D)
                throw new GpuResourceException(this, "A non-2D texture cannot use linear tiling mode");

            if (_info.MipLevels != 1)
                throw new GpuResourceException(this, "Texture linear-tiled texture must have only 1 mip-map level.");

            if (_info.ArrayLayers != 1)
                throw new GpuResourceException(this, "Texture linear-tiled texture must have only 1 array layer.");

            if (_info.Samples != SampleCountFlags.Count1Bit)
                throw new GpuResourceException(this, "Texture linear-tiled texture must have a sample count of 1.");

            if (_info.Usage > (ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit))
                throw new GpuResourceException(this, "A linear-tiled texture must have only source and/or destination transfer bits set. Any other usage flags are invalid.");
        }

        // Does the memory need to be host-visible?
        MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;
        if (Flags.Has(GpuResourceFlags.CpuRead) || Flags.Has(GpuResourceFlags.CpuWrite))
            memFlags |= MemoryPropertyFlags.HostCoherentBit | MemoryPropertyFlags.HostVisibleBit;
        else
            memFlags |= MemoryPropertyFlags.DeviceLocalBit;

        _handle = CreateImageHandle();
        CreateImage(Device, _handle?.SubHandle, memFlags, ref _info, ref _viewInfo);
    }

    protected virtual ResourceHandleVK<Image, ImageHandleVK> CreateImageHandle()
    {
        return new ResourceHandleVK<Image, ImageHandleVK>(this, true, CreateImage);
    }

    protected virtual void CreateImage(DeviceVK device, ImageHandleVK subHandle, MemoryPropertyFlags memFlags, 
        ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
    {
        CreateImage(device, subHandle, memFlags);
    }

    protected virtual void CreateImage(DeviceVK device, ImageHandleVK subHandle, MemoryPropertyFlags memFlags)
    {
        Result r = device.VK.CreateImage(device, _info, null, subHandle.Ptr);
        if (!r.Check(device, () => "Failed to create image resource"))
            return;

        MemoryRequirements memRequirements;
        device.VK.GetImageMemoryRequirements(device, *subHandle.Ptr, &memRequirements);
        subHandle.Memory = device.Memory.Allocate(ref memRequirements, memFlags);

        if (subHandle.Memory == null)
            throw new GpuResourceException(this, "Failed to allocate memory for image resource");

        _viewInfo.Image = *subHandle.Ptr;
        r = device.VK.BindImageMemory(device, *subHandle.Ptr, subHandle.Memory, 0);
        if (!r.Check(device, () => "Failed to bind image memory"))
            return;

        r = device.VK.CreateImageView(device, _viewInfo, null, subHandle.ViewPtr);
        if (!r.Check(device, () => "Failed to create image view"))
            return;
    }

    protected override void OnResizeTexture(ref readonly TextureDimensions dimensions, GpuResourceFormat format)
    {
        throw new NotImplementedException();
    }

    internal void Transition(GraphicsQueueVK cmd, ImageLayout oldLayout, ImageLayout newLayout)
    {
        Transition(cmd, oldLayout, newLayout, ResourceFormat, MipMapCount, ArraySize);
    }

    internal void Transition(GraphicsQueueVK cmd, ImageLayout oldLayout, ImageLayout newLayout, GpuResourceFormat newFormat, uint newMipMapCount, uint newArraySize)
    {
        ImageMemoryBarrier barrier = new ImageMemoryBarrier()
        {
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = *_handle.NativePtr,
            SubresourceRange = _viewInfo.SubresourceRange,
        };

        barrier.SubresourceRange = new ImageSubresourceRange()
        {
            LevelCount = newMipMapCount,
            LayerCount = newArraySize,
            AspectMask = ImageAspectFlags.ColorBit,
            BaseArrayLayer = 0,
            BaseMipLevel = 0
        };

        PipelineStageFlags srcFlags;
        PipelineStageFlags destFlags;

        (barrier.SrcAccessMask, srcFlags) = SetTransitionBarrier(oldLayout);
        (barrier.DstAccessMask, destFlags) = SetTransitionBarrier(newLayout);

        cmd.MemoryBarrier(srcFlags, destFlags, &barrier);
    }

    protected (AccessFlags, PipelineStageFlags) SetTransitionBarrier(ImageLayout layout)
    {
        switch (layout)
        {
            default:
                throw new GpuResourceException(this, $"Unsupported transition image layout '{layout}'.");

            case ImageLayout.SharedPresentKhr:
            case ImageLayout.PresentSrcKhr:
                return (AccessFlags.None, PipelineStageFlags.None);

            case ImageLayout.Undefined:
                return (AccessFlags.None, PipelineStageFlags.TopOfPipeBit);

            case ImageLayout.TransferDstOptimal:
                return (AccessFlags.TransferWriteBit, PipelineStageFlags.TransferBit);

            case ImageLayout.ReadOnlyOptimal:
                return (AccessFlags.ShaderReadBit, PipelineStageFlags.FragmentShaderBit);

            case ImageLayout.ColorAttachmentOptimal:
                return (AccessFlags.ColorAttachmentReadBit, PipelineStageFlags.FragmentShaderBit);

            case ImageLayout.DepthAttachmentOptimal:
                return (AccessFlags.DepthStencilAttachmentReadBit, PipelineStageFlags.FragmentShaderBit);
        }
    }

    protected abstract void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo);

    protected override void OnGraphicsRelease()
    {
        _handle?.Dispose();
        _handle = null;
    }

    public override ResourceHandleVK<Image, ImageHandleVK> Handle => _handle;

    public new DeviceVK Device { get; }
}

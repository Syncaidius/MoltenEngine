using Silk.NET.Direct3D.Compilers;
using System.Reflection.Metadata;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe abstract class TextureVK : GraphicsTexture
    {
        ImageCreateInfo _info;
        ImageViewCreateInfo _viewInfo;

        ImageHandleVK[] _handles;
        ImageHandleVK _curHandle;

        protected TextureVK(GraphicsDevice device, GraphicsTextureType type, TextureDimensions dimensions,
            AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) :
            base(device, type, dimensions, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        { }

        protected override void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID)
        {
            SetHandle(frameBufferIndex);
        }

        protected void SetHandle(uint index)
        {
            _curHandle = _handles[index];
        }

        protected override sealed void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            // In Vulkan, the CPU either has read AND write access, or none at all.
            // If either of the CPU access flags were provided, we need to add both.
            if (Flags.Has(GraphicsResourceFlags.CpuRead) || Flags.Has(GraphicsResourceFlags.CpuWrite))
                Flags |= GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.CpuWrite;

            DeviceVK device = Device as DeviceVK;

            _handles = new ImageHandleVK[frameBufferSize];

            // Don't allocate memory for Image handles if the texture is a swapchain surface.
            // Swapchain image creation/disposal is controlled entirely by the underlying Vulkan implementation.
            bool allocImagePtr = !(this is ISwapChainSurface);
            for (uint i = 0; i < frameBufferSize; i++)
                _handles[i] = new ImageHandleVK(device, allocImagePtr);

            _curHandle = _handles[frameBufferIndex];

            ImageUsageFlags flags = ImageUsageFlags.None;
            if (Flags.Has(GraphicsResourceFlags.GpuRead))
                flags |= ImageUsageFlags.TransferSrcBit;

            if (Flags.Has(GraphicsResourceFlags.GpuWrite))
                flags |= ImageUsageFlags.TransferDstBit;

            if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                flags |= ImageUsageFlags.StorageBit;

            if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
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

            SetCreateInfo(device, ref _info, ref _viewInfo);

            // Creation of images with tiling VK_IMAGE_TILING_LINEAR may not be supported unless other parameters meet all of the constraints
            if (_info.Tiling == ImageTiling.Linear)
            {
                //if (this is DepthSurfaceVK depthSurface)
                //    throw new GraphicsResourceException(this, "A depth surface texture cannot use linear tiling mode");

                if (_info.ImageType != ImageType.Type2D)
                    throw new GraphicsResourceException(this, "A non-2D texture cannot use linear tiling mode");

                if (_info.MipLevels != 1)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have only 1 mip-map level.");

                if (_info.ArrayLayers != 1)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have only 1 array layer.");

                if (_info.Samples != SampleCountFlags.Count1Bit)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have a sample count of 1.");

                if (_info.Usage > (ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit))
                    throw new GraphicsResourceException(this, "A linear-tiled texture must have only source and/or destination transfer bits set. Any other usage flags are invalid.");
            }

            // Does the memory need to be host-visible?
            MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;
            if (Flags.Has(GraphicsResourceFlags.CpuRead) || Flags.Has(GraphicsResourceFlags.CpuWrite))
                memFlags |= MemoryPropertyFlags.HostCoherentBit | MemoryPropertyFlags.HostVisibleBit;
            else
                memFlags |= MemoryPropertyFlags.DeviceLocalBit;

            CreateImages(device, _handles, memFlags, ref _info, ref _viewInfo);
        }

        protected virtual void CreateImages(DeviceVK device, ImageHandleVK[] handles, MemoryPropertyFlags memFlags, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            for (int i = 0; i < handles.Length; i++)
            {
                ImageHandleVK handle = handles[i];
                Result r = device.VK.CreateImage(device, _info, null, handle.NativePtr);
                if (!r.Check(device, () => "Failed to create image resource"))
                    return;
                MemoryRequirements memRequirements;
                device.VK.GetImageMemoryRequirements(device, *handle.NativePtr, &memRequirements);
                handle.Memory = device.Memory.Allocate(ref memRequirements, memFlags);

                if (handle.Memory == null)
                    throw new GraphicsResourceException(this, "Failed to allocate memory for image resource");

                _viewInfo.Image = *handle.NativePtr;
                r = device.VK.BindImageMemory(device, *handle.NativePtr, handle.Memory, 0);
                if (!r.Check(device, () => "Failed to bind image memory"))
                    return;

                r = device.VK.CreateImageView(device, _viewInfo, null, handle.ViewPtr);
                if (!r.Check(device, () => "Failed to create image view"))
                    return;
            }
        }

        protected override void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            throw new NotImplementedException();
        }

        protected override void OnResizeResource(in TextureDimensions dimensions)
        {
            throw new NotImplementedException();
        }

        internal void Transition(GraphicsQueueVK cmd, ImageLayout oldLayout, ImageLayout newLayout)
        {
            Transition(cmd, oldLayout, newLayout, ResourceFormat, MipMapCount, ArraySize);
        }

        internal void Transition(GraphicsQueueVK cmd, ImageLayout oldLayout, ImageLayout newLayout, GraphicsFormat newFormat, uint newMipMapCount, uint newArraySize)
        {
            ImageMemoryBarrier barrier = new ImageMemoryBarrier()
            {
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = *_curHandle.NativePtr,
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
                    throw new GraphicsResourceException(this, $"Unsupported transition image layout '{layout}'.");

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

        protected override void OnGenerateMipMaps(GraphicsQueue queue)
        {
            throw new NotImplementedException();
        }

        protected abstract void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo);

        protected override void OnGraphicsRelease()
        {
            for (int i = 0; i < KnownFrameBufferSize; i++)
                _handles[i].Dispose();

            _curHandle = null;
        }

        public override ImageHandleVK Handle => _curHandle;

        public override unsafe void* SRV => Handle.ViewPtr;

        public override unsafe void* UAV => throw new NotImplementedException();
    }
}

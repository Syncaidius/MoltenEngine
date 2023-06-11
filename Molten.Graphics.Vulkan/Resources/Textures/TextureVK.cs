using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe abstract class TextureVK : GraphicsTexture
    {
        ImageCreateInfo _desc;
        ImageViewCreateInfo _viewDesc;

        ImageView* _view;
        ResourceHandleVK* _handle;
        MemoryAllocationVK _memory;

        protected TextureVK(GraphicsDevice device, GraphicsTextureType type, TextureDimensions dimensions,
            AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, type, dimensions, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
            _handle = ResourceHandleVK.AllocateNew<Image>();
            _view = EngineUtil.Alloc<ImageView>();

            // In Vulkan, the CPU either has read AND write access, or none at all.
            // If either of the CPU access flags were provided, we need to add both.
            if(Flags.Has(GraphicsResourceFlags.CpuRead) || Flags.Has(GraphicsResourceFlags.CpuWrite))
                Flags |= GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.CpuWrite;
        }

        protected void CreateImage()
        {
            DeviceVK device = Device as DeviceVK;

            ImageUsageFlags flags = ImageUsageFlags.None;
            if (Flags.Has(GraphicsResourceFlags.GpuRead))
                flags |= ImageUsageFlags.TransferSrcBit;

            if(Flags.Has(GraphicsResourceFlags.GpuWrite))
                flags |= ImageUsageFlags.TransferDstBit;

            if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                flags |= ImageUsageFlags.StorageBit;

            if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                flags |= ImageUsageFlags.SampledBit;

            _desc = new ImageCreateInfo(StructureType.ImageCreateInfo);
            _desc.Extent.Width = Width;
            _desc.Extent.Height = Height;
            _desc.Extent.Depth = Depth;
            _desc.MipLevels = MipMapCount;
            _desc.ArrayLayers = ArraySize;
            _desc.Format = ResourceFormat.ToApi();
            _desc.Tiling = ImageTiling.Optimal;
            _desc.InitialLayout = ImageLayout.Undefined;
            _desc.Usage = flags;
            _desc.SharingMode = SharingMode.Exclusive;
            _desc.Samples = SampleCountFlags.Count1Bit;
            _desc.Flags = ImageCreateFlags.None;

            // Queue properties are ignored if sharing mode is not VK_SHARING_MODE_CONCURRENT.
            if (_desc.SharingMode == SharingMode.Concurrent)
            {
                _desc.PQueueFamilyIndices = EngineUtil.AllocArray<uint>(1);
                _desc.PQueueFamilyIndices[0] = (Device.Queue as GraphicsQueueVK).Index;
                _desc.QueueFamilyIndexCount = 1;
            }

            _viewDesc = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo);
            _viewDesc.Format = _desc.Format;
            _viewDesc.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
            _viewDesc.SubresourceRange.BaseMipLevel = 0;
            _viewDesc.SubresourceRange.LevelCount = MipMapCount;
            _viewDesc.SubresourceRange.BaseArrayLayer = 0;
            _viewDesc.SubresourceRange.LayerCount = ArraySize;
            _viewDesc.Flags = ImageViewCreateFlags.None;

            SetCreateInfo(device, ref _desc, ref _viewDesc);

            // Creation of images with tiling VK_IMAGE_TILING_LINEAR may not be supported unless other parameters meet all of the constraints
            if (_desc.Tiling == ImageTiling.Linear)
            {
                //if (this is DepthSurfaceVK depthSurface)
                //    throw new GraphicsResourceException(this, "A depth surface texture cannot use linear tiling mode");

                if (_desc.ImageType != ImageType.Type2D)
                    throw new GraphicsResourceException(this, "A non-2D texture cannot use linear tiling mode");

                if(_desc.MipLevels != 1)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have only 1 mip-map level.");

                if(_desc.ArrayLayers != 1)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have only 1 array layer.");

                if (_desc.Samples != SampleCountFlags.Count1Bit)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have a sample count of 1.");

                if (_desc.Usage > (ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit))
                    throw new GraphicsResourceException(this, "A linear-tiled texture must have only source and/or destination transfer bits set. Any other usage flags are invalid.");
            }

            OnCreateImage(device, _handle, ref _desc, ref _viewDesc);
        }

        protected unsafe virtual void OnCreateImage(DeviceVK device, ResourceHandleVK* handle, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            // Does the memory need to be host-visible?
            MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;
            if (Flags.Has(GraphicsResourceFlags.CpuRead) || Flags.Has(GraphicsResourceFlags.CpuWrite))
                memFlags |= MemoryPropertyFlags.HostCoherentBit | MemoryPropertyFlags.HostVisibleBit;
            else
                memFlags |= MemoryPropertyFlags.DeviceLocalBit;

            Image* img = handle->As<Image>();
            Result r = device.VK.CreateImage(device, _desc, null, img);
            if (!r.Check(device, () => "Failed to create image resource"))
                return;

            MemoryRequirements memRequirements;
            device.VK.GetImageMemoryRequirements(device, *(Image*)handle->Ptr, &memRequirements);
            _memory = device.Memory.Allocate(ref memRequirements, memFlags);

            if (_memory == null)
                throw new GraphicsResourceException(this, "Failed to allocate memory for image resource");

            handle->Memory = _memory;
            handle->MemoryFlags = memFlags;
            _viewDesc.Image = *img;

            r = device.VK.BindImageMemory(device, *(Image*)handle->Ptr, _memory.Handle, 0);
            if (!r.Check(device, () => "Failed to bind image memory"))
                return;

            r = device.VK.CreateImageView(device, _viewDesc, null, _view);
            if (!r.Check(device, () => "Failed to create image view"))
                return;
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
                Image = *(Image*)_handle->Ptr,
                SubresourceRange = _viewDesc.SubresourceRange,
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

                case ImageLayout.PresentSrcKhr:
                    return (AccessFlags.ColorAttachmentWriteBit, PipelineStageFlags.None);

                case ImageLayout.Undefined:
                    return (AccessFlags.None, PipelineStageFlags.TopOfPipeBit);

                case ImageLayout.TransferDstOptimal:
                    return (AccessFlags.TransferWriteBit, PipelineStageFlags.TransferBit);

                case ImageLayout.ReadOnlyOptimal:
                    return (AccessFlags.ShaderReadBit, PipelineStageFlags.FragmentShaderBit);

                case ImageLayout.ColorAttachmentOptimal:
                    return (AccessFlags.ColorAttachmentReadBit, PipelineStageFlags.FragmentShaderBit);
            }
        }

        protected override void OnApply(GraphicsQueue queue)
        {
            if (IsDisposed)
                return;

            Image* ptr = _handle->As<Image>();
            if (ptr->Handle == 0)
                CreateImage();

            base.OnApply(queue);
        }

        protected override void OnGenerateMipMaps(GraphicsQueue queue)
        {
            throw new NotImplementedException();
        }

        protected abstract void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo);

        private void DestroyResources()
        {
            DeviceVK device = Device as DeviceVK;
            if (_view != null)
                device.VK.DestroyImageView(device, *_view, null);

            if (_handle->Ptr != null)
                device.VK.DestroyImage(device, *(Image*)_handle->Ptr, null);

            _memory?.Free();
        }

        protected override void OnGraphicsRelease()
        {
            DestroyResources();
            EngineUtil.Free(ref _view);

            _handle->Dispose();
            EngineUtil.Free(ref _handle);
        }

        protected override void OnSetSize()
        {
            throw new NotImplementedException();
        }

        public override unsafe void* Handle => _handle;

        internal unsafe Image* ImageHandle => _handle->As<Image>();

        public override unsafe void* SRV => _view;

        public override unsafe void* UAV => throw new NotImplementedException();
    }
}

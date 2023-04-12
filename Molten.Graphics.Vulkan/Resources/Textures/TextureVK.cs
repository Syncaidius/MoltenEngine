using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public TextureVK(GraphicsDevice device, GraphicsTextureType type, TextureDimensions dimensions,
            AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, type, dimensions, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
            _handle = EngineUtil.Alloc<ResourceHandleVK>();
            _handle->Ptr = EngineUtil.Alloc<Image>();
            _view = EngineUtil.Alloc<ImageView>();

            CreateImage();
        }

        protected void CreateImage()
        {
            DeviceVK device = Device as DeviceVK;

            // Does the memory need to be host-visible?
            MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;
            if (Flags.Has(GraphicsResourceFlags.CpuRead) || Flags.Has(GraphicsResourceFlags.CpuWrite))
                memFlags |= MemoryPropertyFlags.HostCoherentBit | MemoryPropertyFlags.HostVisibleBit;
            else
                memFlags |= MemoryPropertyFlags.DeviceLocalBit;

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

            SetCreateInfo(ref _desc, ref _viewDesc);

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

            if (_desc.ImageType == 0)
                throw new GraphicsResourceException(this, "Image type not set during image creation");

            if (_viewDesc.ViewType == 0)
                throw new GraphicsResourceException(this, "View type not set during image-view creation");

            Image img = new Image();
            Result r = device.VK.CreateImage(device, _desc, null, &img);
            if (r.Check(device, () => "Failed to create image resource"))
                return;

            _handle->Set(img);

            MemoryRequirements memRequirements;
            device.VK.GetImageMemoryRequirements(device, *(Image*)_handle->Ptr, &memRequirements);
            _memory = device.Memory.Allocate(ref memRequirements, memFlags);

            if(_memory == null)
                throw new GraphicsResourceException(this, "Failed to allocate memory for image resource");

            _handle->Memory = _memory;
            r = device.VK.BindImageMemory(device, *(Image*)_handle->Ptr, _memory.Handle, 0);
            if (r.Check(device, () => "Failed to bind image memory"))
                return;

            r = device.VK.CreateImageView(device, _viewDesc, null, _view);
            if (r.Check(device, () => "Failed to create image view"))
                return;
        }

        private void Transition(GraphicsQueueVK cmd, ImageLayout oldLayout, ImageLayout newLayout, GraphicsFormat newFormat, uint newMipMapCount, uint newArraySize)
        {
            ImageMemoryBarrier barrier = new ImageMemoryBarrier(StructureType.ImageMemoryBarrier)
            {
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = *(Image*)_handle->Ptr,
                SubresourceRange = _viewDesc.SubresourceRange,
            };

            barrier.SubresourceRange.LevelCount = newMipMapCount;
            barrier.SubresourceRange.LayerCount = newArraySize;

            PipelineStageFlags srcFlags;
            PipelineStageFlags destFlags;

            if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.None;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                srcFlags = PipelineStageFlags.TopOfPipeBit;
                destFlags = PipelineStageFlags.TransferBit;
            }
            else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcFlags = PipelineStageFlags.TransferBit;
                destFlags = PipelineStageFlags.FragmentShaderBit;
            }
            else
            {
                throw new GraphicsResourceException(this, "Unsupported image layout transition.");
            }

            cmd.MemoryBarrier(srcFlags, destFlags, &barrier);
        }

        protected override void OnGenerateMipMaps(GraphicsQueue cmd)
        {
            throw new NotImplementedException();
        }

        protected abstract void SetCreateInfo(ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo);

        private void DestroyResources()
        {
            DeviceVK device = Device as DeviceVK;
            if (_view != null)
                device.VK.DestroyImageView(device, *_view, null);

            if (_handle->Ptr != null)
                device.VK.DestroyImage(device, *(Image*)_handle->Ptr, null);

            _memory?.Free();
        }

        public override void GraphicsRelease()
        {
            DestroyResources();
            EngineUtil.Free(ref _view);
            EngineUtil.Free(ref _handle->Ptr);
            EngineUtil.Free(ref _handle);
        }

        protected override void OnSetSize()
        {
            throw new NotImplementedException();
        }

        public override unsafe void* Handle => _handle;

        public override unsafe void* SRV => _view;

        public override unsafe void* UAV => throw new NotImplementedException();
    }
}

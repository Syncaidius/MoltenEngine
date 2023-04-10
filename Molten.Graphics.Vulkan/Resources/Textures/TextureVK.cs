using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe abstract class TextureVK : GraphicsTexture
    {
        Image* _native;
        ImageView* _view;

        public TextureVK(GraphicsDevice device, 
            uint width, uint height, uint depth, 
            uint mipCount, uint arraySize, 
            AntiAliasLevel aaLevel,
            MSAAQuality sampleQuality, 
            GraphicsFormat format, 
            GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, width, height, depth, mipCount, arraySize, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
            _native = EngineUtil.Alloc<Image>();
            _view = EngineUtil.Alloc<ImageView>();

            CreateImage();
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

            ImageCreateInfo imgInfo = new ImageCreateInfo(StructureType.ImageCreateInfo);
            imgInfo.Extent.Width = Width;
            imgInfo.Extent.Height = Height;
            imgInfo.Extent.Depth = Depth;
            imgInfo.MipLevels = MipMapCount;
            imgInfo.ArrayLayers = ArraySize;
            imgInfo.Format = ResourceFormat.ToApi();
            imgInfo.Tiling = ImageTiling.Optimal;
            imgInfo.InitialLayout = ImageLayout.Undefined;
            imgInfo.Usage = flags;
            imgInfo.SharingMode = SharingMode.Exclusive;
            imgInfo.Samples = SampleCountFlags.Count1Bit;
            imgInfo.Flags = ImageCreateFlags.None;

            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo);
            viewInfo.Format = imgInfo.Format;
            viewInfo.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
            viewInfo.SubresourceRange.BaseMipLevel = 0;
            viewInfo.SubresourceRange.LevelCount = MipMapCount;
            viewInfo.SubresourceRange.BaseArrayLayer = 0;
            viewInfo.SubresourceRange.LayerCount = ArraySize;
            viewInfo.Flags = ImageViewCreateFlags.None;

            SetCreateInfo(ref imgInfo, ref viewInfo);

            if(imgInfo.ImageType == 0)
                throw new GraphicsResourceException(this, "Image type not set during image creation");

            if (viewInfo.ViewType == 0)
                throw new GraphicsResourceException(this, "View type not set during image-view creation");

            Result r = device.VK.CreateImage(device, &imgInfo, null, _native);
            if (r.Check(device, () => "Failed to create image resource"))
                return;

            r = device.VK.CreateImageView(device, &viewInfo, null, _view);
            if (r.Check(device, () => "Failed to create image view"))
                return;
        }

        protected abstract void SetCreateInfo(ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo);

        private void DestroyResources()
        {
            DeviceVK device = Device as DeviceVK;
            if (_view != null)
                device.VK.DestroyImageView(device, *_view, null);
            if (_native != null)
                device.VK.DestroyImage(device, *_native, null);
        }

        public override void GraphicsRelease()
        {
            DestroyResources();
            EngineUtil.Free(ref _view);
            EngineUtil.Free(ref _native);
        }

        protected override void OnSetSize()
        {
            throw new NotImplementedException();
        }

        public override unsafe void* Handle => _native;

        public override unsafe void* SRV => _view;

        public override unsafe void* UAV => throw new NotImplementedException();
    }
}

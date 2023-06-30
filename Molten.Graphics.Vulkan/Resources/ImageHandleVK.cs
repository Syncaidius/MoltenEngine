using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe class ImageHandleVK : ResourceSubHandleVK<Image>
    {
        internal ImageView* _ptrView;

        internal override void Initialize(DeviceVK device, bool isAllocated)
        {
            base.Initialize(device, isAllocated);
            _ptrView = EngineUtil.Alloc<ImageView>();
        }

        internal override void Release(DeviceVK device, bool isAllocated)
        {
            if (_ptrView != null)
            {
                device.VK.DestroyImageView(device, *_ptrView, null);
                EngineUtil.Free(ref _ptrView);
            }

            if (Ptr != null && !isAllocated)
                device.VK.DestroyImage(device, *Ptr, null);

            base.Release(device, isAllocated);
        }

        public ref ImageView* ViewPtr => ref _ptrView;
    }
}

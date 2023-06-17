using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe class ImageHandleVK : ResourceHandleVK<Image>
    {
        internal ImageView* _ptrView;

        internal ImageHandleVK(DeviceVK device) :
            base(device)
        {
            _ptrView = EngineUtil.Alloc<ImageView>();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (_ptrView != null)
            {
                Device.VK.DestroyImageView(Device, *_ptrView, null);
                EngineUtil.Free(ref _ptrView);
            }

            if (Ptr != null)
                Device.VK.DestroyImage(Device, *NativePtr, null);

            base.Dispose();
        }

        public ImageView* ViewPtr => _ptrView;
    }
}

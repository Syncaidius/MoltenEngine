using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics.Vulkan
{
    public unsafe class BufferHandleVK : ResourceHandleVK<Buffer>
    {
        internal BufferView* _ptrView;

        internal BufferHandleVK(DeviceVK device) :
            base(device)
        {
            _ptrView = EngineUtil.Alloc<BufferView>();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (_ptrView != null)
            {
                Device.VK.DestroyBufferView(Device, *_ptrView, null);
                EngineUtil.Free(ref _ptrView);
            }

            if (Ptr != null)
                Device.VK.DestroyBuffer(Device, *NativePtr, null);

            base.Dispose();
        }

        public BufferView* ViewPtr => _ptrView;
    }
}

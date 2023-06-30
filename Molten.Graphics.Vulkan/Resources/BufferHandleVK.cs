using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics.Vulkan
{
    public unsafe class BufferHandleVK : ResourceSubHandleVK<Buffer>
    {
        BufferView* _ptrView;

        internal override void Initialize(DeviceVK device, bool isAllocated)
        {
            base.Initialize(device, isAllocated);
            _ptrView = EngineUtil.Alloc<BufferView>();
        }

        internal override void Release(DeviceVK device, bool isAllocated)
        {
            if (_ptrView != null)
            {
                device.VK.DestroyBufferView(device, *_ptrView, null);
                EngineUtil.Free(ref _ptrView);
            }

            if (Ptr != null)
                device.VK.DestroyBuffer(device, *Ptr, null);

            base.Release(device, isAllocated);
        }

        public BufferView* ViewPtr => _ptrView;
    }
}

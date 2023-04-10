using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe struct ResourceHandleVK
    {
        public void* Ptr;

        public DeviceMemory Memory;
    }
}

using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe struct ResourceHandleVK
    {
        public void* Ptr;

        public DeviceMemory* Memory;
    }
}

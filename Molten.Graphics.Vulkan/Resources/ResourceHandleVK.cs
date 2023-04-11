using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe struct ResourceHandleVK
    {
        public void* Ptr;

        public DeviceMemory Memory;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(T handle)
            where T : unmanaged
        {
            ((T*)Ptr)[0] = handle;
        }
    }
}

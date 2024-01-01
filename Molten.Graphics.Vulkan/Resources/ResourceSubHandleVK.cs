namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// Represents a sub-handle for a <see cref="ResourceHandleVK{SH}"/>. 
    /// <para>These are used to manage multiple copies of a resource during a frame, usually as a result of <see cref="ResourceHandleVK.Discard"/> calls.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe abstract class ResourceSubHandleVK<T>
    where T : unmanaged
    {
        internal T* Ptr;

        internal MemoryAllocationVK Memory;

        internal virtual void Initialize(DeviceVK device, bool isAllocated)
        {
            if (isAllocated)
                Ptr = EngineUtil.Alloc<T>();
        }

        internal virtual void Release(DeviceVK device, bool isAllocated)
        {
            if (isAllocated)
                EngineUtil.Free(ref Ptr);
        }

        /// <summary>
        /// The ID of the frame in which the sub-handle was last used.
        /// </summary>
        internal ulong LastFrameUsed;
    }
}

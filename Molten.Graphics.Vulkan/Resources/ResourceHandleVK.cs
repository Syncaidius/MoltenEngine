using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe struct ResourceHandleVK : IDisposable
    {
        bool _allocated;

        public void* _ptr;

        public DeviceMemory Memory;

        public MemoryPropertyFlags MemoryFlags;

        /// <summary>
        /// Allocates a resource handle for the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static ResourceHandleVK* AllocateNew<T>()
            where T : unmanaged
        {
            ResourceHandleVK* handle = EngineUtil.Alloc<ResourceHandleVK>();
            EngineUtil.Clear(handle);

            handle->_ptr = EngineUtil.Alloc<T>();
            EngineUtil.Clear((T*)handle->_ptr);

            return handle;
        }

        internal void SetValue<T>(T ptr)
            where T : unmanaged
        {
            T* tPtr = (T*)_ptr;
            tPtr[0] = ptr;
        }

        /// <summary>
        /// Returns the underlying <see cref="Ptr"/> as a pointer to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which to convert the underlying pointer.</typeparam>
        /// <returns></returns>
        internal T* As<T>() 
            where T : unmanaged
        {
            return (T*)_ptr;
        }

        public void Dispose()
        {
            if(IsDisposed)
                throw new ObjectDisposedException("The current ResourceHandleVK is already disposed.");

            EngineUtil.Free(ref _ptr);
        }


        /// <summary>
        /// Gets whether or not the current <see cref="ResourceHandleVK"/> has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the underlying pointer.
        /// </summary>
        public void* Ptr => _ptr;
    }
}

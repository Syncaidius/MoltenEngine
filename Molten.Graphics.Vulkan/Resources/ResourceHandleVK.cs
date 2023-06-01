using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe struct ResourceHandleVK : IDisposable
    {
        bool _allocated;

        public void* _ptr;

        public DeviceMemory Memory;

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
            handle->Allocate<T>();
            return handle;
        }

        /// <summary>
        /// Allocates memory to fit the specified type and stores its pointer in <see cref="Ptr"/>.
        /// </summary>
        /// <typeparam name="T">The type for which to allocate memory.</typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        internal T* Allocate<T>()
            where T : unmanaged
        {
            if(_allocated)
                throw new InvalidOperationException("Cannot allocate a resource handle that has already been allocated.");

            if(IsDisposed)
                throw new ObjectDisposedException("Cannot allocate a disposed ResourceHandleVK.");

            _allocated = true;
            _ptr = EngineUtil.Alloc<T>();
            EngineUtil.Clear((T*)_ptr);
            return (T*)_ptr;
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

            if (_allocated)
            {
                EngineUtil.Free(ref _ptr);
                _allocated = false;
            }
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

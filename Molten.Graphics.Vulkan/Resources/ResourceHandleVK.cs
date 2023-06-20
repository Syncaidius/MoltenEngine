using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe abstract class ResourceHandleVK : GraphicsResourceHandle
    {
        bool _disposed;

        internal MemoryAllocationVK Memory;

        internal ResourceHandleVK(GraphicsResource resource) : base(resource)
        {
            Device = resource.Device as DeviceVK;
        }

        /// <summary>
        /// Disposes of the current Vulkan resource handle and frees <see cref="Memory"/> if assigned.
        /// </summary>
        public override void Dispose()
        {
            if(_disposed)
                throw new ObjectDisposedException("The current ResourceHandleVK is already disposed.");

            _disposed = true;
            Memory?.Free();
        }

        internal DeviceVK Device { get; }
    }

    public unsafe abstract class ResourceHandleVK<T> : ResourceHandleVK
        where T : unmanaged
    {
        T* _ptr;

        protected ResourceHandleVK(GraphicsResource resource, bool allocate) :
            base(resource)
        {
            IsAllocated = allocate;

            if (IsAllocated)
                _ptr = EngineUtil.Alloc<T>();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();

            if (IsAllocated)
                EngineUtil.Free(ref _ptr);
        }

        public override unsafe void* Ptr => _ptr;

        internal ref T* NativePtr => ref _ptr;

        protected bool IsAllocated { get; set; }
    }
}

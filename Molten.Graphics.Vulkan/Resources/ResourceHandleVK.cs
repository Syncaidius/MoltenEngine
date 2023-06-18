using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe abstract class ResourceHandleVK: GraphicsResourceHandle
    {
        bool _disposed;

        internal MemoryAllocationVK Memory;

        internal ResourceHandleVK(DeviceVK device)
        {
            Device = device;
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

        internal ResourceHandleVK(DeviceVK device) :
            base(device)
        {
            _ptr = EngineUtil.Alloc<T>();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            EngineUtil.Free(ref _ptr);
        }

        public override unsafe void* Ptr => _ptr;

        internal T* NativePtr => _ptr;
    }
}

using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

public unsafe abstract class ResourceHandleVK : GpuResourceHandle
{
    bool _disposed;

    protected ResourceHandleVK(GpuResource resource) : base(resource)
    {
        Device = resource.Device as DeviceVK;
    }

    internal abstract void UpdateUsage(ulong frameID);

    /// <summary>
    /// Discards the current sub-handle and switches to another one that is not in-use by the GPU. 
    /// If no existing sub-handles are available, a new one will be created.
    /// </summary>
    internal abstract void Discard();

    /// <summary>
    /// Disposes of the current Vulkan resource handle and frees <see cref="Memory"/> if assigned.
    /// </summary>
    public override void Dispose()
    {
        if(_disposed)
            throw new ObjectDisposedException("The current ResourceHandleVK is already disposed.");

        _disposed = true;
        OnDispose();
    }

    protected abstract void OnDispose();

    internal DeviceVK Device { get; }

    /// <summary>
    /// Gets the current memory allocation. This may automatically update or change if the resource is discarded via <see cref="Discard"/>.
    /// </summary>
    internal abstract MemoryAllocationVK Memory { get; }
}

public unsafe class ResourceHandleVK<T, SH> : ResourceHandleVK
    where T : unmanaged
    where SH : ResourceSubHandleVK<T>, new()
{
    internal delegate void DiscardCallback(DeviceVK device, SH subHandle, MemoryPropertyFlags memFlags);

    DiscardCallback _discardCallback;
    List<SH> _subHandles;
    int _subIndex;
    SH _sub;

    internal ResourceHandleVK(GpuResource resource, bool allocate, DiscardCallback discardCallback) :
        base(resource)
    {
        _subHandles = new List<SH>();
        _discardCallback = discardCallback;
        IsAllocated = allocate;

        if (IsAllocated)
            AllocateSubHandle();
    }

    private void AllocateSubHandle()
    {
        _sub = new SH();
        _sub.Initialize(Device, IsAllocated);
        _subHandles.Add(_sub);
    }

    internal override sealed void UpdateUsage(ulong frameID)
    {
        if (_subHandles[0].LastFrameUsed != frameID)
            _subIndex = 0;

        _sub.LastFrameUsed = frameID;
        ulong maxAge = Device.FrameBufferSize * 16;

        // Check all of the other sub-handles and see if any of them are unused.
        for(int i = _subHandles.Count - 1; i >= 0; i--)
        {
            SH sub = _subHandles[i];
            ulong age = frameID - sub.LastFrameUsed;

            // If the sub-handle exceeds the maximum age, release and remove it.
            if (age > maxAge)
            {
                sub.Release(Device, IsAllocated);
                _subHandles.RemoveAt(i);
            }
        }
    }

    internal override sealed void Discard()
    {
        _subIndex++;

        // Do we need a new sub-handle or can we use an existing one?
        if (_subIndex == _subHandles.Count)
        {
            AllocateSubHandle();
            _discardCallback(Device, _sub, _subHandles[0].Memory.Flags);
        }
        else
        {
            _sub = _subHandles[_subIndex];
        }

        _sub.LastFrameUsed = Device.Renderer.FrameID;
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {
        if (IsAllocated)
        {
            foreach (ResourceSubHandleVK<T> sub in _subHandles)
                sub.Release(Device, IsAllocated);
        }
    }

    internal ref T* NativePtr => ref _sub.Ptr;

    protected bool IsAllocated { get; set; }

    internal override sealed MemoryAllocationVK Memory => _sub.Memory;

    internal SH SubHandle => _sub;

    /// <summary>
    /// Gets the number of sub-handles currently assigned to the <see cref="ResourceHandleVK{SH}"/>.
    /// <para>This only increases if <see cref="Discard"/> is called and there are insufficient existing sub-handles.</para>
    /// </summary>
    internal int SubHandleCount => _subHandles.Count;
}

namespace Molten.Graphics.DX12;

/// <summary>
/// Represents a view for a DirectX 12 resource, such as a shader resource (SRV), unordered-access (UAV) resource or render-target (RTV).
/// </summary>
/// <typeparam name="D">Underlying view description type.</typeparam>
public abstract class ViewDX12<D> : IDisposable
    where D : unmanaged
{
    HeapHandleDX12 _heapHandle;

    protected ViewDX12(ResourceHandleDX12 handle) {
        Handle = handle;
    }

    internal void Create(ref D desc, uint numDescriptors = 1)
    {
        if (_heapHandle.NumSlots != numDescriptors)
            _heapHandle.Heap.Free(ref _heapHandle);

        if (_heapHandle.Heap == null)
            _heapHandle = OnAllocateHandle(numDescriptors);

        OnCreate(ref desc);
    }

    private protected abstract HeapHandleDX12 OnAllocateHandle(uint numDescriptors);

    protected abstract void OnCreate(ref D desc);

    public void Dispose()
    {
        _heapHandle.Heap?.Free(ref _heapHandle);
    }

    /// <summary>
    /// Gets the parent <see cref="ResourceHandleDX12"/>.
    /// </summary>
    internal ResourceHandleDX12 Handle { get; private set; }

    /// <summary>
    /// Gets the CPU-based descriptor handle for the view.
    /// </summary>
    internal ref HeapHandleDX12 DescriptorHandle => ref _heapHandle;

    /// <summary>
    /// Gets the allocated slot within the <see cref="ParentHeap"/>.
    /// </summary>
    internal uint HeapSlotIndex { get; set; }

    /// <summary>
    /// Gets the parent <see cref="DescriptorHeapDX12"/>.
    /// </summary>
    internal DescriptorHeapDX12 ParentHeap { get; set; }
}

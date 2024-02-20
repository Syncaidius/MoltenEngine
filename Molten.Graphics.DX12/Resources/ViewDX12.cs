using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

/// <summary>
/// Represents a view for a DirectX 12 resource, such as a shader resource (SRV), unordered-access (UAV) resource or render-target (RTV).
/// </summary>
/// <typeparam name="D">Underlying view description type.</typeparam>
internal class ViewDX12<D> : IDisposable
    where D : unmanaged
{
    D _desc;
    HeapHandleDX12 _heapHandle;

    internal ViewDX12(ResourceHandleDX12 handle)
    {
        Handle = handle;
    }

    public void Dispose()
    {
        _heapHandle.Heap?.Free(ref _heapHandle);
    }

    internal ref D Desc => ref _desc;

    /// <summary>
    /// Gets the parent <see cref="ResourceHandleDX12"/>.
    /// </summary>
    internal ResourceHandleDX12 Handle { get; }

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

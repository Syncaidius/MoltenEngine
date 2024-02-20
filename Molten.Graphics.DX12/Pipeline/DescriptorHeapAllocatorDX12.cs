using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class DescriptorHeapAllocatorDX12 : GraphicsObject<DeviceDX12>
{
    List<DescriptorHeapDX12> _heaps;
    DescriptorHeapType _type;
    DescriptorHeapDesc _desc;

    internal DescriptorHeapAllocatorDX12(DeviceDX12 device, DescriptorHeapType type, DescriptorHeapFlags flags) : 
        base(device)
    {
        _heaps = new List<DescriptorHeapDX12>();
        _type = type;
        _desc = new DescriptorHeapDesc()
        {
            NodeMask = 0,
            Type = type,
            Flags = flags,
            NumDescriptors = 64,
        };
    }

    internal HeapHandleDX12 Allocate(uint numDescriptors)
    {
        if(numDescriptors > _desc.NumDescriptors)
            throw new InvalidOperationException($"The number of requested descriptors exceeds the capacity of a heap ({_desc.NumDescriptors}).");

        HeapHandleDX12 handle;
        DescriptorHeapDX12 heap;

        // Attempt to allocate from existing heaps.
        for (int i = 0; i < _heaps.Count; i++)
        {
            heap = _heaps[i];
            if(heap.TryAllocate(numDescriptors, out handle))
                return handle;
        }

        // Allocate a new heap.
        heap = new DescriptorHeapDX12(Device, _desc);
        _heaps.Add(heap);
        if(!heap.TryAllocate(numDescriptors, out handle))
            throw new InvalidOperationException("Failed to allocate a new descriptor heap.");

        return handle;
    }

    protected override void OnGraphicsRelease()
    {
        for(int i = 0; i < _heaps.Count; i++)
            _heaps[i].Dispose(true);
    }
}

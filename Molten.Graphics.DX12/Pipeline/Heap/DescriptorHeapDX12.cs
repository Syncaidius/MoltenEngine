using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class DescriptorHeapDX12 : GraphicsObject<DeviceDX12>
{
    ID3D12DescriptorHeap* _handle;
    uint _incrementSize;
    ulong _availabilityMask;
    bool _isGpuVisible;
    uint _capacity;
    CpuDescriptorHandle _cpuStartHandle;

    internal DescriptorHeapDX12(DeviceDX12 device, DescriptorHeapDesc desc) : 
        base(device)
    {
        Guid guid = ID3D12DescriptorHeap.Guid;
        void* ptr = null;
        HResult hr = device.Ptr->CreateDescriptorHeap(desc, &guid, &ptr);

        if(!device.Log.CheckResult(hr, () => $"Failed to create descriptor heap with capacity '{desc.NumDescriptors}'"))
            return;

        _capacity = desc.NumDescriptors;
        _handle = (ID3D12DescriptorHeap*)ptr;
        _incrementSize = device.Ptr->GetDescriptorHandleIncrementSize(desc.Type);
        _isGpuVisible = (desc.Flags & DescriptorHeapFlags.ShaderVisible) == DescriptorHeapFlags.ShaderVisible;
        _cpuStartHandle = _handle->GetCPUDescriptorHandleForHeapStart();
    }

    internal bool TryAllocate(uint numSlots, out HeapHandleDX12 handle)
    {
        handle = default;

        // If the heap is full, return false.
        if (_availabilityMask != ulong.MaxValue)
        {
            uint startIndex = 0; // The first slot of the requested range.
            ulong mask;
            ulong slotMask = 0;

            for (int i = 0; i < _capacity; i++)
            {
                mask = (1UL << i);

                // If the slot is already taken, reset the free slot count.
                if ((_availabilityMask & mask) == mask)
                {
                    startIndex = (uint)i;
                }
                else
                {
                    slotMask |= mask;
                    if ((i + 1) - startIndex == numSlots)
                    {
                        _availabilityMask |= slotMask;
                        handle.CpuHandle = new CpuDescriptorHandle(_cpuStartHandle.Ptr + (startIndex * _incrementSize));
                        handle.Heap = this;
                        handle.StartIndex = startIndex;
                        handle.NumSlots = numSlots;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    internal void Free(ref HeapHandleDX12 handle)
    {
        ulong mask = 0;
        for(int i = (int)handle.StartIndex; i < handle.NumSlots; i++)
            mask |= (1UL << i);

        _availabilityMask &= ~mask;
        handle.Heap = null;
    }

    internal GpuDescriptorHandle GetGpuHandle()
    {
        if (!_isGpuVisible)
            throw new InvalidOperationException("Cannot get a GPU handle from a non-GPU visible heap.");

        return _handle->GetGPUDescriptorHandleForHeapStart();
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _handle);
    }

    public static implicit operator ID3D12DescriptorHeap*(DescriptorHeapDX12 heap)
    {
        return heap._handle;
    }
}

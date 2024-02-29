using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class UAViewDX12 : ViewDX12<UnorderedAccessViewDesc>
{
    public UAViewDX12(ResourceHandleDX12 handle) : base(handle)
    {
    }

    protected override unsafe void OnCreate(ref UnorderedAccessViewDesc desc, ID3D12Resource1* resource, ref CpuDescriptorHandle heapHandle, uint resourceIndex)
    {
        // TODO Add support for counter resources.

        Handle.Device.Ptr->CreateUnorderedAccessView((ID3D12Resource*)resource, null, desc, heapHandle);
    }

    private protected override void OnAllocateHandle(uint numDescriptors, out HeapHandleDX12 handle)
    {
        handle = Handle.Device.Heap.GetResourceHandle(numDescriptors);
    }
}

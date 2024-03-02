using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class CBViewDX12 : ViewDX12<ConstantBufferViewDesc>
{
    public CBViewDX12(ResourceHandleDX12 handle) : 
        base(handle) { }

    protected override unsafe void OnCreate(ref ConstantBufferViewDesc desc, ID3D12Resource1* resource, ref CpuDescriptorHandle heapHandle, uint resourceIndex)
    {
        Handle.Device.Handle->CreateConstantBufferView(desc, heapHandle);
    }

    private protected override void OnAllocateHandle(uint numDescriptors, out HeapHandleDX12 handle)
    {
        handle = Handle.Device.Heap.GetResourceHandle(numDescriptors);
    }
}

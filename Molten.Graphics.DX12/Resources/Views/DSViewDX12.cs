using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class DSViewDX12 : ViewDX12<DepthStencilViewDesc>
{
    public DSViewDX12(ResourceHandleDX12 handle) : 
        base(handle) { }

    protected override unsafe void OnCreate(ref DepthStencilViewDesc desc, ID3D12Resource1* resource, ref CpuDescriptorHandle heapHandle, uint resourceIndex)
    {
        Handle.Device.Ptr->CreateDepthStencilView((ID3D12Resource*)resource, desc, heapHandle);
    }

    private protected override void OnAllocateHandle(uint numDescriptors, out HeapHandleDX12 handle)
    {
        handle = Handle.Device.Heap.GetDepthHandle(numDescriptors);
    }
}

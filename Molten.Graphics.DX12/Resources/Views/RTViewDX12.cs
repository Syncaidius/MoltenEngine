using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class RTViewDX12 : ViewDX12<RenderTargetViewDesc>
{
    public RTViewDX12(ResourceHandleDX12 handle) :
        base(handle)
    { }

    protected override unsafe void OnCreate(ref RenderTargetViewDesc desc, ID3D12Resource1* resource, ref CpuDescriptorHandle heapHandle, uint resourceIndex)
    {
        Handle.Device.Handle->CreateRenderTargetView((ID3D12Resource*)resource, desc, heapHandle);
    }

    private protected override void OnAllocateHandle(uint numDescriptors, out HeapHandleDX12 handle)
    {
        handle = Handle.Device.Heap.GetRTHandle(numDescriptors);
    }
}

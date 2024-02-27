using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class DSViewDX12 : ViewDX12<DepthStencilViewDesc>
{
    public DSViewDX12(ResourceHandleDX12 handle) : 
        base(handle) { }

    protected unsafe override void OnCreate(ref DepthStencilViewDesc desc)
    {
        Handle.Device.Ptr->CreateDepthStencilView(Handle.Ptr, desc, DescriptorHandle.CpuHandle);
    }

    private protected override HeapHandleDX12 OnAllocateHandle(uint numDescriptors)
    {
        return Handle.Device.Heap.GetDepthHandle(numDescriptors);
    }
}

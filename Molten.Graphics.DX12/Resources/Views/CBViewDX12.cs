using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class CBViewDX12 : ViewDX12<ConstantBufferViewDesc>
{
    public CBViewDX12(ResourceHandleDX12 handle) : 
        base(handle) { }

    protected unsafe override void OnCreate(ref ConstantBufferViewDesc desc)
    {
        Handle.Device.Ptr->CreateConstantBufferView(desc, DescriptorHandle.CpuHandle);
    }

    private protected override HeapHandleDX12 OnAllocateHandle(uint numDescriptors)
    {
        return Handle.Device.Heap.GetResourceHandle(numDescriptors);
    }
}

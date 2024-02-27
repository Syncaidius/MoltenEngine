using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class RTViewDX12 : ViewDX12<RenderTargetViewDesc>
{
    public RTViewDX12(ResourceHandleDX12 handle) :
        base(handle)
    { }

    protected unsafe override void OnCreate(ref RenderTargetViewDesc desc)
    {
        Handle.Device.Ptr->CreateRenderTargetView(Handle.Ptr, desc, DescriptorHandle.CpuHandle);
    }

    private protected override HeapHandleDX12 OnAllocateHandle(uint numDescriptors)
    {
        return Handle.Device.Heap.GetRTHandle(numDescriptors);
    }
}

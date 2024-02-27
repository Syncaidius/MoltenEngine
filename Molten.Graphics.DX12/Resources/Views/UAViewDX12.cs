using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class UAViewDX12 : ViewDX12<UnorderedAccessViewDesc>
{
    public UAViewDX12(ResourceHandleDX12 handle) : base(handle)
    {
    }

    protected unsafe override void OnCreate(ref UnorderedAccessViewDesc desc)
    {
        // TODO Add support for counter resources.

        Handle.Device.Ptr->CreateUnorderedAccessView(Handle.Ptr, null, desc, DescriptorHandle.CpuHandle);
    }

    private protected override HeapHandleDX12 OnAllocateHandle(uint numDescriptors)
    {
        return Handle.Device.Heap.GetResourceHandle(numDescriptors);
    }
}

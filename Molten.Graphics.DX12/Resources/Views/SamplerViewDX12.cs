using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class SamplerViewDX12 : ViewDX12<SamplerDesc>
{
    public SamplerViewDX12(ResourceHandleDX12 handle) : base(handle)
    { }

    protected unsafe override void OnCreate(ref SamplerDesc desc)
    {
        Handle.Device.Ptr->CreateSampler(desc, DescriptorHandle.CpuHandle);
    }

    private protected override void OnAllocateHandle(uint numDescriptors, out HeapHandleDX12 handle)
    {
        handle = Handle.Device.Heap.GetSamplerHandle(numDescriptors);
    }
}

using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class SamplerViewDX12 : ViewDX12<SamplerDesc>
{
    public SamplerViewDX12(ResourceHandleDX12 handle) : base(handle)
    { }

    protected override unsafe void OnCreate(ref SamplerDesc desc, ID3D12Resource1* resource, ref CpuDescriptorHandle heapHandle, uint resourceIndex)
    {
        // TODO Samplers do not use ID3D12Resource - Refactor this to be more efficient.

        Handle.Device.Ptr->CreateSampler(desc, heapHandle);
    }

    private protected override void OnAllocateHandle(uint numDescriptors, out HeapHandleDX12 handle)
    {
        handle = Handle.Device.Heap.GetSamplerHandle(numDescriptors);
    }
}

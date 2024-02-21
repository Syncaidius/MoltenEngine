using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class DescriptorHeapManagerDX12 : GraphicsObject<DeviceDX12>
{
    DescriptorHeapAllocatorDX12 _resourceHeap;
    DescriptorHeapAllocatorDX12 _samplerHeap;
    DescriptorHeapAllocatorDX12 _dsvHeap;
    DescriptorHeapAllocatorDX12 _rtvHeap;

    DescriptorHeapDX12 _gpuResourceHeap;
    DescriptorHeapDX12 _gpuSamplerHeap;

    /// <summary>
    /// Creates a new instance of <see cref="DescriptorHeapAllocatorDX12"/>.
    /// </summary>
    /// <param name="device">The device that the heap manager belongs to.</param>
    /// <param name="heapCapacity">The number of slots to provision within each individual heap.</param>
    internal DescriptorHeapManagerDX12(DeviceDX12 device) :
        base(device)
    {
        _resourceHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.CbvSrvUav, DescriptorHeapFlags.None);
        _samplerHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Sampler, DescriptorHeapFlags.None);
        _dsvHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Dsv, DescriptorHeapFlags.None);
        _rtvHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Rtv, DescriptorHeapFlags.None);

        _gpuResourceHeap = new DescriptorHeapDX12(device, new DescriptorHeapDesc()
        {
            NodeMask = 0,
            Type = DescriptorHeapType.CbvSrvUav,
            Flags = DescriptorHeapFlags.ShaderVisible,
            NumDescriptors = 1024,
        });

        _gpuSamplerHeap = new DescriptorHeapDX12(device, new DescriptorHeapDesc()
        {
            NodeMask = 0,
            Type = DescriptorHeapType.Sampler,
            Flags = DescriptorHeapFlags.ShaderVisible,
            NumDescriptors = 512,
        });
    }

    internal unsafe void Allocate(ViewDX12<ShaderResourceViewDesc> view)
    {
        view.DescriptorHandle = _resourceHeap.Allocate(1);
    }

    internal unsafe void Allocate(ViewDX12<UnorderedAccessViewDesc> view)
    {
        // TODO Add support for counter resources.
        view.DescriptorHandle = _resourceHeap.Allocate(1);
        Device.Ptr->CreateUnorderedAccessView(view.Handle.Ptr, null, view.Desc, view.DescriptorHandle.CpuHandle);
    }

    internal unsafe void Allocate(ViewDX12<RenderTargetViewDesc> view)
    {
        view.DescriptorHandle = _rtvHeap.Allocate(1);
        Device.Ptr->CreateRenderTargetView(view.Handle.Ptr, view.Desc, view.DescriptorHandle.CpuHandle);
    }

    internal unsafe void Allocate(ViewDX12<DepthStencilViewDesc> view)
    {
        view.DescriptorHandle = _dsvHeap.Allocate(1);
        Device.Ptr->CreateDepthStencilView(view.Handle.Ptr, view.Desc, view.DescriptorHandle.CpuHandle);
    }

    internal unsafe void Allocate(ViewDX12<SamplerDesc> view)
    {
        view.DescriptorHandle = _samplerHeap.Allocate(1);
        Device.Ptr->CreateSampler(view.Desc, view.DescriptorHandle.CpuHandle);
    }

    internal unsafe void Allocate(ViewDX12<ConstantBufferViewDesc> view)
    {
        view.DescriptorHandle = _resourceHeap.Allocate(1);
        Device.Ptr->CreateConstantBufferView(view.Desc, view.DescriptorHandle.CpuHandle);
    }

    /// <summary>
    /// Consolidates all of the CPU-side descriptors into a single GPU descriptor heap ready for use.
    /// </summary>
    internal void PrepareGpuHeap(ShaderPassDX12 pass)
    {
        int index = 0;

        GpuDescriptorHandle gpuHandle = _gpuResourceHeap.GetGpuHandle();
        
        // TODO Iterate over pass resources

        // Populate SRV, UAV, and CBV descriptors first.
        // TODO Pull descriptor info from our pass, render targets, samplers, depth-stencil, etc.
    }

    protected override void OnGraphicsRelease()
    {
        _resourceHeap.Dispose(true);
        _samplerHeap.Dispose(true);
        _dsvHeap.Dispose(true);
        _rtvHeap.Dispose(true);

        _gpuResourceHeap.Dispose(true);
        _gpuSamplerHeap.Dispose(true);
    }
}
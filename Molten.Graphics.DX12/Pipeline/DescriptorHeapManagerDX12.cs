using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class DescriptorHeapManagerDX12 : GraphicsObject<DeviceDX12>
{
    DescriptorHeapAllocatorDX12 _resourceHeap;
    DescriptorHeapAllocatorDX12 _samplerHeap;
    DescriptorHeapAllocatorDX12 _dsvHeap;
    DescriptorHeapAllocatorDX12 _rtvHeap;

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
    }

    internal void Allocate(ViewDX12<ShaderResourceViewDesc> view)
    {
        view.DescriptorHandle = _resourceHeap.Allocate(1);
    }

    internal void Allocate(ViewDX12<UnorderedAccessViewDesc> view)
    {
        view.DescriptorHandle = _resourceHeap.Allocate(1);
    }

    internal void Allocate(ViewDX12<RenderTargetViewDesc> view)
    {
        view.DescriptorHandle = _rtvHeap.Allocate(1);
    }

    internal void Allocate(ViewDX12<DepthStencilViewDesc> view)
    {
        view.DescriptorHandle = _dsvHeap.Allocate(1);
    }

    internal void Allocate(ViewDX12<SamplerDesc> view)
    {
        view.DescriptorHandle = _samplerHeap.Allocate(1);
    }

    /// <summary>
    /// Consolidates all of the CPU-side descriptors into a single GPU descriptor heap ready for use.
    /// </summary>
    internal void PrepareGpuHeap(ShaderPassDX12 pass)
    {
        // TODO Pull descriptor info from our pass, render targets, samplers, depth-stencil, etc.
    }

    protected override void OnGraphicsRelease()
    {
        _resourceHeap.Dispose(true);
        _samplerHeap.Dispose(true);
        _dsvHeap.Dispose(true);
        _rtvHeap.Dispose(true);
    }
}
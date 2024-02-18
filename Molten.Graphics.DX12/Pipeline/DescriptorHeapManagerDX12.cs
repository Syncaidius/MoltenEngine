using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class DescriptorHeapManagerDX12 : GraphicsObject<DeviceDX12>
{
    DescriptorHeapAllocatorDX12 ResourceHeap { get; }

    DescriptorHeapAllocatorDX12 SamplerHeap { get; }

    DescriptorHeapAllocatorDX12 DsvHeap { get; }

    DescriptorHeapAllocatorDX12 RtvHeap { get; }

    /// <summary>
    /// Creates a new instance of <see cref="DescriptorHeapAllocatorDX12"/>.
    /// </summary>
    /// <param name="device">The device that the heap manager belongs to.</param>
    /// <param name="heapCapacity">The number of slots to provision within each individual heap.</param>
    internal DescriptorHeapManagerDX12(DeviceDX12 device) :
        base(device)
    {
        ResourceHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.CbvSrvUav, DescriptorHeapFlags.None);
        SamplerHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Sampler, DescriptorHeapFlags.None);
        DsvHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Dsv, DescriptorHeapFlags.None);
        RtvHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Rtv, DescriptorHeapFlags.None);
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
        ResourceHeap.Dispose(true);
        SamplerHeap.Dispose(true);
        DsvHeap.Dispose(true);
        RtvHeap.Dispose(true);
    }
}
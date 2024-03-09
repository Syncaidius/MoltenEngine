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

    internal HeapHandleDX12 GetResourceHandle(uint numDescriptors)
    {
        return _resourceHeap.Allocate(numDescriptors);
    }

    internal HeapHandleDX12 GetRTHandle(uint numDescriptors)
    {
        return _rtvHeap.Allocate(numDescriptors);
    }

    internal HeapHandleDX12 GetDepthHandle(uint numDescriptors)
    {
        return _dsvHeap.Allocate(numDescriptors);
    }

    internal HeapHandleDX12 GetSamplerHandle(uint numDescriptors)
    {
        return _samplerHeap.Allocate(numDescriptors);
    }

    /// <summary>
    /// Consolidates all of the CPU-side descriptors into a single GPU descriptor heap ready for use.
    /// </summary>
    internal unsafe void PrepareGpuHeap(ShaderPassDX12 pass, GraphicsCommandListDX12 cmd)
    {
        int index = 0;

        GpuDescriptorHandle gpuHandle = _gpuResourceHeap.GetGpuHandle();

        // TODO Iterate over pass resources

        // Populate SRV, UAV, and CBV descriptors first.
        // TODO Pull descriptor info from our pass, render targets, samplers, depth-stencil, etc.

        cmd.Handle->SetGraphicsRootDescriptorTable(0, gpuHandle);
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
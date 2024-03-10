using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class DescriptorHeapManagerDX12 : GraphicsObject<DeviceDX12>
{
    const int RESOURCE_HEAP_SIZE = 512;
    const int SAMPLER_HEAP_SIZE = 256;

    DescriptorHeapAllocatorDX12 _resourceHeap;
    DescriptorHeapAllocatorDX12 _samplerHeap;
    DescriptorHeapAllocatorDX12 _dsvHeap;
    DescriptorHeapAllocatorDX12 _rtvHeap;

    GpuFrameBuffer<DescriptorHeapDX12> _gpuResourceHeap;
    GpuFrameBuffer<DescriptorHeapDX12> _gpuSamplerHeap;

    /// <summary>
    /// Creates a new instance of <see cref="DescriptorHeapAllocatorDX12"/>.
    /// </summary>
    /// <param name="device">The device that the heap manager belongs to.</param>
    /// <param name="heapCapacity">The number of slots to provision within each individual heap.</param>
    internal unsafe DescriptorHeapManagerDX12(DeviceDX12 device) :
        base(device)
    {
        _resourceHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.CbvSrvUav, DescriptorHeapFlags.None);
        _samplerHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Sampler, DescriptorHeapFlags.None);
        _dsvHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Dsv, DescriptorHeapFlags.None);
        _rtvHeap = new DescriptorHeapAllocatorDX12(device, DescriptorHeapType.Rtv, DescriptorHeapFlags.None);

        _gpuResourceHeap = new GpuFrameBuffer<DescriptorHeapDX12>(device, (creationDevice) =>
        {
            return new DescriptorHeapDX12(creationDevice as DeviceDX12, new DescriptorHeapDesc()
            {
                NodeMask = 0,
                Type = DescriptorHeapType.CbvSrvUav,
                Flags = DescriptorHeapFlags.ShaderVisible,
                NumDescriptors = RESOURCE_HEAP_SIZE,
            });
        });

        _gpuSamplerHeap = new GpuFrameBuffer<DescriptorHeapDX12>(device, (creationDevice) =>
        {
            return new DescriptorHeapDX12(creationDevice as DeviceDX12, new DescriptorHeapDesc()
            {
                NodeMask = 0,
                Type = DescriptorHeapType.Sampler,
                Flags = DescriptorHeapFlags.ShaderVisible,
                NumDescriptors = SAMPLER_HEAP_SIZE,
            });
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
        DeviceDX12 device = pass.Device as DeviceDX12;
        DescriptorHeapDX12 resHeap = _gpuResourceHeap.Prepare();
        CpuDescriptorHandle gpuResHandle = resHeap.CpuStartHandle;

        DescriptorHeapDX12 samplerHeap = _gpuSamplerHeap.Prepare();
        CpuDescriptorHandle gpuSamplerHandle = samplerHeap.CpuStartHandle;

        // TODO Replace this once DX11 is removed and resources can be created during instantiation instead of during Apply().
        // Apply resources.
        for (int i = 0; i < pass.Bindings.Resources.Length; i++)
        {
            ShaderBindType bindType = (ShaderBindType)i;
            ref ShaderBind<ShaderResourceVariable>[] resources = ref pass.Bindings.Resources[i];
            for (int r = 0; r < resources.Length; r++)
            {
                ref ShaderBind<ShaderResourceVariable> bind = ref resources[r];
                bind.Object.Resource?.Apply(cmd.Queue);
            }
        }

        // Iterate over pass resources
        for (int i = 0; i < pass.Bindings.Resources.Length; i++)
        {
            ShaderBindType bindType = (ShaderBindType)i;
            ref ShaderBind<ShaderResourceVariable>[] resources = ref pass.Bindings.Resources[i];
            for (int r = 0; r < resources.Length; r++)
            {
                ref ShaderBind<ShaderResourceVariable> bind = ref resources[r];
                if (bind.Object?.Value != null)
                {
                    // TODO Improve this
                    ResourceHandleDX12 resHandle = bind.Object.Resource.Handle as ResourceHandleDX12;
                    CpuDescriptorHandle cpuHandle = default;
                    switch (bindType)
                    {
                        case ShaderBindType.ConstantBuffer:
                            CBHandleDX12 cbHandle = resHandle as CBHandleDX12;
                            cpuHandle = cbHandle.CBV.CpuHandle;
                            break;

                        case ShaderBindType.Resource:
                            cpuHandle = resHandle.SRV.CpuHandle;
                            break;

                        case ShaderBindType.UnorderedAccess:
                            cpuHandle = resHandle.UAV.CpuHandle;
                            break;
                    }

                    if (cpuHandle.Ptr != 0)
                        device.Handle->CopyDescriptorsSimple(1, gpuResHandle, cpuHandle, DescriptorHeapType.CbvSrvUav);

                    // Increment GPU heap handle
                    gpuResHandle.Ptr += resHeap.IncrementSize;
                }
            }
        }

        // Iterate over pass samplers
        for (int i = 0; i < pass.Bindings.Samplers.Length; i++)
        {
            ref ShaderBind<ShaderSamplerVariable> bind = ref pass.Bindings.Samplers[i];
            if (!bind.Object.IsImmutable && bind.Object?.Value != null)
            {
                ShaderSampler heapSampler = bind.Object.Sampler;
                // TODO _handleBuffer[index] = heapSampler.View.CpuHandle; 
                gpuSamplerHandle.Ptr += samplerHeap.IncrementSize;
            }
        }

        // Populate SRV, UAV, and CBV descriptors first.
        // TODO Pull descriptor info from our pass, render targets, samplers, depth-stencil, etc.

        if (gpuResHandle.Ptr != resHeap.CpuStartHandle.Ptr 
            && gpuSamplerHandle.Ptr != samplerHeap.CpuStartHandle.Ptr)
        {
            ID3D12DescriptorHeap** pHeaps = stackalloc ID3D12DescriptorHeap*[2] { resHeap.Handle, samplerHeap.Handle };

            cmd.Handle->SetDescriptorHeaps(2, pHeaps);
            cmd.Handle->SetGraphicsRootDescriptorTable(0, resHeap.GetGpuHandle());
            cmd.Handle->SetGraphicsRootDescriptorTable(1, samplerHeap.GetGpuHandle());
        }else if (gpuResHandle.Ptr != resHeap.CpuStartHandle.Ptr)
        {
            ID3D12DescriptorHeap** pHeaps = stackalloc ID3D12DescriptorHeap*[1] { resHeap.Handle };
            cmd.Handle->SetDescriptorHeaps(1, pHeaps);
            cmd.Handle->SetGraphicsRootDescriptorTable(0, resHeap.GetGpuHandle());
        }
        else if (gpuSamplerHandle.Ptr != samplerHeap.CpuStartHandle.Ptr)
        {
            ID3D12DescriptorHeap** pHeaps = stackalloc ID3D12DescriptorHeap*[1] { samplerHeap.Handle };
            cmd.Handle->SetDescriptorHeaps(1, pHeaps);
            cmd.Handle->SetGraphicsRootDescriptorTable(0, samplerHeap.GetGpuHandle());
        }
    }

    protected unsafe override void OnGraphicsRelease()
    {
        _resourceHeap.Dispose(true);
        _samplerHeap.Dispose(true);
        _dsvHeap.Dispose(true);
        _rtvHeap.Dispose(true);

        _gpuResourceHeap.Dispose(true);
        _gpuSamplerHeap.Dispose(true);
    }
}
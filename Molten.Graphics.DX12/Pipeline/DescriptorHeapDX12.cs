using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using System.Runtime.CompilerServices;

namespace Molten.Graphics.DX12;

internal unsafe class DescriptorHeapDX12 : GraphicsObject<DeviceDX12>
{
    public class DescriptorHeapIndexException(DescriptorHeapDX12 heap, uint requestedIndex) : 
        Exception($"Requested descriptor heap index is out of range.")
    {
        public DescriptorHeapDX12 Heap => heap;

        public uint RequestedIndex => requestedIndex;
    }

    internal unsafe delegate void IterateCallback<T>(ref T handle, void* ptr) where T : unmanaged;

    ID3D12DescriptorHeap* _handle;
    DescriptorHeapDesc _desc;
    CpuDescriptorHandle _cpuStartHandle;
    GpuDescriptorHandle _gpuStartHandle;

    internal DescriptorHeapDX12(DeviceDX12 device, uint capacity, DescriptorHeapType type, DescriptorHeapFlags flags) : 
        base(device)
    {
        _desc = new DescriptorHeapDesc()
        {
            NodeMask = 0,
            Type = type,
            Flags = flags,
            NumDescriptors = capacity,
        };

        Guid guid = ID3D12DescriptorHeap.Guid;
        void* ptr = null;

        HResult hr;
        fixed (DescriptorHeapDesc* ptrDesc = &_desc)
            hr = device.Ptr->CreateDescriptorHeap(ptrDesc, &guid, &ptr);

        if(!device.Log.CheckResult(hr, () => $"Failed to create descriptor heap with capacity '{capacity}'"))
            return;

        _handle = (ID3D12DescriptorHeap*)ptr;
        _cpuStartHandle = _handle->GetCPUDescriptorHandleForHeapStart();
        IncrementSize = device.Ptr->GetDescriptorHandleIncrementSize(type);

        // Only create a GPU start handle if the heap is shader visible.
        if(flags.HasFlag(DescriptorHeapFlags.ShaderVisible))
            _gpuStartHandle = _handle->GetGPUDescriptorHandleForHeapStart();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal CpuDescriptorHandle GetCpuHandle(uint index)
    {
        if (index > Capacity)
            throw new DescriptorHeapIndexException(this, index);

        return new CpuDescriptorHandle(_cpuStartHandle.Ptr + (index * IncrementSize));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal GpuDescriptorHandle GetGpuHandle(uint index)
    {
        if (index > Capacity)
            throw new DescriptorHeapIndexException(this, index);

        return new GpuDescriptorHandle(_gpuStartHandle.Ptr + (index * IncrementSize));
    }

    internal void Assign(ResourceViewDX12<ShaderResourceViewDesc> view, uint handleIndex)
    {
        CpuDescriptorHandle handle = GetCpuHandle(handleIndex);
        Device.Ptr->CreateShaderResourceView(view.Handle, view.Desc, handle);
    }

    internal void Assign(ResourceViewDX12<UnorderedAccessViewDesc> view, uint handleIndex)
    {
        CpuDescriptorHandle handle = GetCpuHandle(handleIndex);
        Device.Ptr->CreateUnorderedAccessView(view.Handle, null, view.Desc, handle);
    }

    internal void Assign(ResourceViewDX12<RenderTargetViewDesc> view, uint handleIndex)
    {
        CpuDescriptorHandle handle = GetCpuHandle(handleIndex);
        Device.Ptr->CreateRenderTargetView(view.Handle, view.Desc, handle);
    }

    internal void Assign(ResourceViewDX12<DepthStencilViewDesc> view, uint handleIndex)
    {
        CpuDescriptorHandle handle = GetCpuHandle(handleIndex);
        Device.Ptr->CreateDepthStencilView(view.Handle, view.Desc, handle);
    }

    internal void Assign(ResourceViewDX12<ConstantBufferViewDesc> view, uint handleIndex)
    {
        CpuDescriptorHandle handle = GetCpuHandle(handleIndex);
        Device.Ptr->CreateConstantBufferView(view.Desc, handle);
    }

    internal void Assign(ResourceViewDX12<SamplerDesc> view, uint handleIndex)
    {
        CpuDescriptorHandle handle = GetCpuHandle(handleIndex);
        Device.Ptr->CreateSampler(view.Desc, handle);
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _handle);
    }

    public ref readonly uint Capacity => ref _desc.NumDescriptors;

    public ref readonly DescriptorHeapType Type => ref _desc.Type;

    public ref readonly DescriptorHeapFlags Flags => ref _desc.Flags;

    public uint IncrementSize { get; }
}

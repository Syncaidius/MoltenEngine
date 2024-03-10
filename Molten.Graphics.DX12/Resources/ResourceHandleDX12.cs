using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class ResourceHandleDX12 : GpuResourceHandle
{
    ID3D12Resource1*[] _ptr;

    internal ResourceHandleDX12(GpuResource resource, params ID3D12Resource1*[] ptr) : base(resource)
    {
        if (ptr == null || ptr.Length == 0)
            throw new ArgumentException("Resource handle must contain at least one resource pointer.");

        SetResources(ptr);
        Device = resource.Device as DeviceDX12;

        if (!resource.Flags.Has(GpuResourceFlags.DenyShaderAccess))
            SRV = new SRViewDX12(this);

        if(resource.Flags.Has(GpuResourceFlags.UnorderedAccess))
            UAV = new UAViewDX12(this);
    }

    internal ResourceHandleDX12(GpuResource resource, ID3D12Resource1** ptr, uint numResources) : base(resource)
    {
        if (numResources == 0)
            throw new ArgumentException("Resource handle must contain at least one resource pointer.");

        SetResources(ptr, numResources);
        Device = resource.Device as DeviceDX12;

        if (!resource.Flags.Has(GpuResourceFlags.DenyShaderAccess))
            SRV = new SRViewDX12(this);

        if (resource.Flags.Has(GpuResourceFlags.UnorderedAccess))
            UAV = new UAViewDX12(this);
    }

    public void SetResources(params ID3D12Resource1*[] ptr)
    {
        _ptr = ptr;
    }

    public void SetResources(ID3D12Resource1** ptr, uint numResources)
    {
        if(_ptr == null || _ptr.Length != numResources)
            _ptr = new ID3D12Resource1*[numResources];

        for (int i = 0; i < numResources; i++)
            _ptr[i] = ptr[i];
    }

    public override void Dispose()
    {
        SRV?.Dispose();
        UAV?.Dispose();

        for (int i = 0; i < _ptr.Length; i++)
            NativeUtil.ReleasePtr(ref _ptr[i]);
    }

    public static implicit operator ID3D12Resource1*(ResourceHandleDX12 handle)
    {
        return handle._ptr[handle.Index];
    }

    public static implicit operator ID3D12Resource*(ResourceHandleDX12 handle)
    {
        return (ID3D12Resource*)handle._ptr[handle.Index];
    }

    internal SRViewDX12 SRV { get; }

    internal UAViewDX12 UAV { get; }

    internal DeviceDX12 Device { get; }

    internal unsafe ID3D12Resource1* Ptr1 => _ptr[Index];

    internal unsafe ID3D12Resource* Ptr => (ID3D12Resource*)_ptr[Index];

    /// <summary>
    /// The current resource pointer index. This is the one that will be used by default when the handle is passed to the D3D12 API.
    /// </summary>
    internal uint Index { get; set; }

    /// <summary>
    /// The number of indexable resources in the handle.
    /// </summary>
    internal uint NumResources => (uint)_ptr.Length;

    /// <summary>
    /// Gets or sets the resource pointer at the specified index.
    /// </summary>
    /// <param name="index">The resource pointer index.</param>
    /// <returns></returns>
    internal ID3D12Resource1* this[uint index]
    {
        get => _ptr[index];
        set => _ptr[index] = value;
    }
}

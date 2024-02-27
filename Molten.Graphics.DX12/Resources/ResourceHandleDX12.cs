using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class ResourceHandleDX12 : GraphicsResourceHandle
{
    ID3D12Resource1*[] _ptr;

    internal ResourceHandleDX12(GraphicsResource resource, params ID3D12Resource1*[] ptr) : base(resource)
    {
        _ptr = ptr;
        Device = resource.Device as DeviceDX12;

        if (!resource.Flags.Has(GraphicsResourceFlags.DenyShaderAccess))
            SRV = new SRViewDX12(this);

        if(resource.Flags.Has(GraphicsResourceFlags.UnorderedAccess))
            UAV = new UAViewDX12(this);
    }

    internal void UpdateResource(ID3D12Resource1* ptr, uint ptrIndex = 0)
    {
        _ptr[ptrIndex] = ptr;
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
        return handle._ptr[handle.PtrIndex];
    }

    public static implicit operator ID3D12Resource*(ResourceHandleDX12 handle)
    {
        return (ID3D12Resource*)handle._ptr[handle.PtrIndex];
    }

    internal SRViewDX12 SRV { get; }

    internal UAViewDX12 UAV { get; }

    internal DeviceDX12 Device { get; }

    public unsafe ID3D12Resource1* Ptr1 => _ptr[PtrIndex];

    public unsafe ID3D12Resource* Ptr => (ID3D12Resource*)_ptr[PtrIndex];

    public uint PtrIndex { get; set; }

    public int NumResources => _ptr.Length;
}

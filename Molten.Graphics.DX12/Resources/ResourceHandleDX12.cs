using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12
{
    public abstract unsafe class ResourceHandleDX12 : GraphicsResourceHandle
    {
        public static implicit operator ID3D12Resource*(ResourceHandleDX12 handle)
        {
            return (ID3D12Resource*)handle.Ptr;
        }

        internal ResourceHandleDX12(GraphicsResource resource) : base(resource)
        {
            Device = resource.Device as DeviceDX12;
            SRV = new ResourceViewDX12<ShaderResourceViewDesc>(this);
            UAV = new ResourceViewDX12<UnorderedAccessViewDesc>(this);
        }

        internal ResourceViewDX12<ShaderResourceViewDesc> SRV { get; }

        internal ResourceViewDX12<UnorderedAccessViewDesc> UAV { get; }

        internal DeviceDX12 Device { get; }
    }

    public unsafe class ResourceHandleDX12<T> : ResourceHandleDX12
        where T : unmanaged
    {
        T* _ptr;

        internal ResourceHandleDX12(GraphicsResource resource) :
            base(resource)
        { }

        public override void Dispose()
        {
            SilkUtil.ReleasePtr(ref _ptr);
        }

        public static implicit operator T*(ResourceHandleDX12<T> handle)
        {
            return handle._ptr;
        }

        public override unsafe void* Ptr => _ptr;

        internal ref T* NativePtr => ref _ptr;
    }
}

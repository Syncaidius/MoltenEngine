using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class RootSignatureDX12 : GraphicsObject<DeviceDX12>
{
    ID3D12RootSignature* _handle;

    internal RootSignatureDX12(DeviceDX12 device, ID3D12RootSignature* handle) :
        base(device)
    {
        _handle = handle;
    }

    public static implicit operator ID3D12RootSignature*(RootSignatureDX12 sig) => sig._handle;

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _handle);
    }

    public ref readonly ID3D12RootSignature* Handle => ref _handle;
}

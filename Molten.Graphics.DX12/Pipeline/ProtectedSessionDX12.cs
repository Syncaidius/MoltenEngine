using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
public unsafe class ProtectedSessionDX12 : GpuObject<DeviceDX12>
{
    ID3D12ProtectedResourceSession* _ptr;
    ID3D12Fence* _statusFence;

    internal ProtectedSessionDX12(DeviceDX12 device) : 
        base(device)
    {
        ID3D12ProtectedResourceSession* session = null;
        Guid guid = ID3D12ProtectedResourceSession.Guid;
        void* ptr = null;
        ProtectedResourceSessionDesc pDesc = new()
        {
            NodeMask = 0,
            Flags = ProtectedResourceSessionFlags.None,
        };
        device.Handle->CreateProtectedResourceSession(&pDesc, &guid, &ptr);
        _ptr = session;

        void* ptrFence = null;
        Guid guidFence = ID3D12Fence.Guid;
        _ptr->GetStatusFence(&guidFence, &ptrFence);
        _statusFence = (ID3D12Fence*)ptrFence;
    }

    internal ProtectedSessionStatus GetStatus()
    {
        return _ptr->GetSessionStatus();
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _statusFence);
        NativeUtil.ReleasePtr(ref _ptr);
    }

    public static implicit operator ID3D12ProtectedResourceSession*(ProtectedSessionDX12 session)
    {
        return session != null ? session._ptr : null;
    }

    public static implicit operator ID3D12ProtectedSession*(ProtectedSessionDX12 session)
    {
        return session != null ? (ID3D12ProtectedSession*)session._ptr : null;
    }
}

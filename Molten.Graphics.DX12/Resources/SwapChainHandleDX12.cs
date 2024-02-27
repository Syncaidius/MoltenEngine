using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal unsafe class SwapChainHandleDX12 : ResourceHandleDX12
{
    ID3D12Resource1** _handles; 
    uint _handleCount;

    internal SwapChainHandleDX12(GraphicsResource resource, ID3D12Resource1** ptr, uint numHandles) :
        base(resource, ptr[0])
    {
        SetHandles(ptr, numHandles);
    }

    internal void SetHandles(ID3D12Resource1** handles, uint numHandles)
    {
        _handles = handles;
        _handleCount = numHandles;
    }

    internal void SetHandleIndex(uint index)
    {
        SetHandle(_handles[index]);
    }

    public override void Dispose()
    {
        if(_handles != null)
        for(int i = 0; i < _handleCount; i++)
            NativeUtil.ReleasePtr(ref _handles[i]);

        EngineUtil.FreePtrArray(ref _handles);
        base.Dispose();
    }
}

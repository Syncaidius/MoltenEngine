using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class GraphicsCommandListDX12 : CommandListDX12<ID3D12GraphicsCommandList>
{
    internal GraphicsCommandListDX12(CommandAllocatorDX12 allocator, ID3D12GraphicsCommandList* handle) :
        base(allocator, handle)
    { }

    internal void Reset(CommandAllocatorDX12 allocator, PipelineStateDX12 initialState)
    {
        ID3D12PipelineState* pState = initialState != null ? initialState.Handle : null;
        Handle->Reset(allocator.Handle, pState);
    }

    public void CopyResource(GraphicsResource dst, GraphicsResource src)
    {
        Handle->CopyResource((ResourceHandleDX12)dst.Handle, (ResourceHandleDX12)src.Handle);
    }

    public void Close()
    {
        Handle->Close();
    }

    public override void Free()
    {
        throw new NotImplementedException();
    }
}

using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class GraphicsCommandListDX12 : CommandListDX12<ID3D12GraphicsCommandList>
{
    internal GraphicsCommandListDX12(CommandAllocatorDX12 allocator, ID3D12GraphicsCommandList* handle) :
        base(allocator, handle)
    { }

    public void Close()
    {
        Handle->Close();
    }
}

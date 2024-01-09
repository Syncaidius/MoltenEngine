using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public abstract class CommandListDX12 : GraphicsCommandList
{
    protected CommandListDX12(CommandQueueDX12 queue, CommandAllocatorDX12 allocator) : base(queue) { }

    protected override void OnGraphicsRelease()
    {
        Allocator.Unallocate(this);
    }

    /// <summary>
    /// Gets the parent <see cref="CommandAllocatorDX12"/> from which the current <see cref="CommandListDX12"/> was allocated.
    /// </summary>
    internal CommandAllocatorDX12 Allocator { get; }

    public unsafe abstract ID3D12CommandList* BaseHandle { get; }

    public CommandListType Type => Allocator.Type;
}

internal abstract unsafe class CommandListDX12<T> : CommandListDX12
    where T : unmanaged
{
    T* _handle;

    internal CommandListDX12(CommandAllocatorDX12 allocator, T* handle) : 
        base(allocator.Queue, allocator)
    {
        _handle = handle;
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _handle);
        base.OnGraphicsRelease();
    }

    public static implicit operator ID3D12CommandList*(CommandListDX12<T> cmd) => (ID3D12CommandList*)cmd._handle;

    public override unsafe ID3D12CommandList* BaseHandle => (ID3D12CommandList*)_handle;

    protected ref T* Handle => ref _handle;
}

using Molten.Collections;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class CommandAllocatorDX12 : GraphicsObject<DeviceDX12>
{
    ID3D12CommandAllocator* _handle;
    ThreadedList<CommandListDX12> _allocated;

    internal CommandAllocatorDX12(GraphicsQueueDX12 queue, CommandListType type) : base(queue.Device)
    {
        Guid guid = ID3D12CommandAllocator.Guid;
        Type = type;
        Queue = queue;

        void* ptr = null;
        HResult hr = Device.Handle->CreateCommandAllocator(CommandListType.Direct, &guid, &ptr);
        if (!queue.Device.Log.CheckResult(hr, () => "Failed to create command allocator"))
            hr.Throw();

        _handle = (ID3D12CommandAllocator*)ptr;
        _allocated = new ThreadedList<CommandListDX12>();
    }

    internal GraphicsCommandListDX12 Allocate(ID3D12PipelineState* pInitialState = null /*TODO Properly provide a PipelineStateDX12*/)
    {
        void* ptr = null;
        Guid guid = ID3D12GraphicsCommandList.Guid;
        HResult hr = Device.Handle->CreateCommandList(0, Type, _handle, pInitialState, &guid, &ptr);
        if (!Device.Log.CheckResult(hr, () => $"Failed to allocate {Type} command list"))
            hr.Throw();

        // TODO Check free list.

        GraphicsCommandListDX12 list = new GraphicsCommandListDX12(this, (ID3D12GraphicsCommandList*) ptr);
        _allocated.Add(list);

        return list;
    }

    internal void Unallocate(CommandListDX12 cmd)
    {
        if(cmd.Allocator != this)
            throw new InvalidOperationException("Command list does not belong to this allocator.");

        _allocated.Remove(cmd);
    }

    protected override void OnGraphicsRelease()
    {
        _allocated.ForReverse(1, (index, cmd) => cmd.Dispose(true));
        NativeUtil.ReleasePtr(ref _handle);
    }

    public CommandListType Type { get; }

    internal GraphicsQueueDX12 Queue { get; }

    internal ID3D12CommandAllocator* Handle => _handle;
}

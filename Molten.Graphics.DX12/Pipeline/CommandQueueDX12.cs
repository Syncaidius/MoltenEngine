using Molten.Collections;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class CommandQueueDX12 : GpuCommandQueue<DeviceDX12>
{
    CommandQueueDesc _desc;
    ID3D12CommandQueue* _handle;
    CommandAllocatorDX12 _currentCmdAllocator;
    CommandListDX12 _prevCmdList;
    Interlocker _lockerExecute;

    internal CommandQueueDX12(Logger log, DeviceDX12 device, DeviceBuilderDX12 builder, ref CommandQueueDesc desc) : 
        base(device)
    {
        _desc = desc;
        Log = log;
        Device = device;

        Initialize(builder);
    }

    private void Initialize(DeviceBuilderDX12 builder)
    {
        Guid cmdGuid = ID3D12CommandQueue.Guid;
        void* cmdQueue = null;

        DeviceDX12 device = Device; 
        HResult r = device.Handle->CreateCommandQueue(_desc, &cmdGuid, &cmdQueue);
        if (!device.Log.CheckResult(r))
        {
            Log.Error($"Failed to initialize '{_desc.Type}' command queue");
            return;
        }

        Log.WriteLine($"Initialized '{_desc.Type}' command queue");

        _handle = (ID3D12CommandQueue*)cmdQueue;
        _submittedLists = new ThreadedList<CommandListDX12>();
    }

    public override GpuCommandList GetCommandList(GpuCommandListFlags flags = GpuCommandListFlags.None)
    {
        throw new NotImplementedException();
    }

    public override void Reset(GpuCommandList list)
    {
        CommandListDX12 cmd = (CommandListDX12)list;
        cmd.Handle->Reset(_currentCmdAllocator.Handle, null);
    }

    public override void BeginFrame()
    {
        throw new NotImplementedException();
    }

    public override void EndFrame()
    {
        throw new NotImplementedException();
    }

    public override void Execute(GpuCommandList cmd)
    {
        if(cmd.HasBegan)
            throw new GpuCommandListException(cmd, "Cannot execute a command list that has not been closed.");

        CommandListDX12 cmdDx12 = (CommandListDX12)cmd;
        ID3D12CommandList** lists = stackalloc ID3D12CommandList*[] { cmdDx12.BaseHandle };

        _lockerExecute.Lock();

        // Tell the GPU to wait for the previous command list to execute before executing the new one.
        if (_prevCmdList != null)
        {
            ulong fenceValue = _prevCmdList.Fence.Signal(this);
            _handle->Wait(_prevCmdList.Fence.Handle, fenceValue);
        }

        _handle->ExecuteCommandLists(1, lists);
        _prevCmdList = cmdDx12;
        _lockerExecute.Unlock();
    }

    /// <inheritdoc />
    public override bool Wait(GpuFence fence, ulong nsTimeout = ulong.MaxValue)
    {
        FenceDX12 fenceDX12 = (FenceDX12)fence;
        return fenceDX12.Wait(this, nsTimeout);
    }

    protected override void OnDispose(bool immediate)
    {
        NativeUtil.ReleasePtr(ref _handle);
    }

    internal ID3D12CommandQueue* Handle => _handle;

    internal Logger Log { get; }

    internal new DeviceDX12 Device { get; }
}

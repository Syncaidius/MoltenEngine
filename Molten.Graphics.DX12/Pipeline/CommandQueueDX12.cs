using Molten.Collections;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class CommandQueueDX12 : GpuCommandQueue<DeviceDX12>
{
    CommandQueueDesc _desc;
    ID3D12CommandQueue* _handle;
    GpuFrameBuffer<CommandAllocatorDX12> _cmdAllocators;
    CommandAllocatorDX12 _currentCmdAllocator;
    ThreadedList<CommandListDX12> _submittedLists;

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
        _cmdAllocators = new GpuFrameBuffer<CommandAllocatorDX12>(Device, (device) =>
        {
            return new CommandAllocatorDX12(this, CommandListType.Direct);
        });
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
        _currentCmdAllocator = _cmdAllocators.Prepare();
    }

    public override void EndFrame()
    {
        throw new NotImplementedException();
    }

    public override void Execute(GpuCommandList list)
    {
        CommandListDX12 cmd = (CommandListDX12)list;

        cmd.Close();
        ID3D12CommandList** lists = stackalloc ID3D12CommandList*[] { cmd.BaseHandle };
        _handle->ExecuteCommandLists(1, lists);
    }

    /// <inheritdoc />
    public override bool Wait(GpuFence fence, ulong nsTimeout = ulong.MaxValue)
    {
        FenceDX12 fenceDX12 = (FenceDX12)fence;
        return fenceDX12.Wait(this, nsTimeout);
    }

    protected override void OnDispose(bool immediate)
    {
        _cmdAllocators?.Dispose(true);
        NativeUtil.ReleasePtr(ref _handle);
    }

    internal ID3D12CommandQueue* Handle => _handle;

    internal Logger Log { get; }

    internal new DeviceDX12 Device { get; }
}

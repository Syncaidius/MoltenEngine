using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Molten.Graphics.Vulkan;

public unsafe class CommandQueueVK : GpuCommandQueue<DeviceVK>
{
    DeviceVK _device;
    Vk _vk;
    CommandPoolVK _poolFrame;
    CommandPoolVK _poolTransient;

    CommandListVK _prevCmdList;
    Interlocker _lockerExecute;

    internal CommandQueueVK(RendererVK renderer, DeviceVK device, uint familyIndex, Queue queue, uint queueIndex, SupportedCommandSet set) :
        base(device)
    {
        _vk = renderer.VK;
        Log = renderer.Log;
        Flags = set.CapabilityFlags;
        _device = device;
        FamilyIndex = familyIndex;
        Index = queueIndex;
        Native = queue;
        Set = set;

        _lockerExecute = new Interlocker();
        _poolFrame = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit, 1);
        _poolTransient = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit | CommandPoolCreateFlags.TransientBit, 5);
    }

    public override void BeginFrame()
    {
        throw new NotImplementedException();
    }

    public override void EndFrame()
    {
        throw new NotImplementedException();
    }

    public override void Reset(GpuCommandList list)
    {
        throw new NotImplementedException();
    }

    public override GpuCommandList GetCommandList(GpuCommandListFlags flags = GpuCommandListFlags.None)
    {
        throw new NotImplementedException();
    }

    public override bool Wait(GpuFence fence, ulong nsTimeout = ulong.MaxValue)
    {
        FenceVK fenceVK = fence as FenceVK;
        return fenceVK.Wait(nsTimeout);
    }

    public override unsafe void Begin(GpuCommandListFlags flags)
    {
        base.Begin();

        CommandBufferLevel level = flags.Has(GpuCommandListFlags.Deferred) ?
            CommandBufferLevel.Secondary :
            CommandBufferLevel.Primary;

        CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
        beginInfo.Flags = CommandBufferUsageFlags.None;

        if(flags.Has(GpuCommandListFlags.SingleSubmit))
            beginInfo.Flags |= CommandBufferUsageFlags.OneTimeSubmitBit;

        _cmd = _poolFrame.Allocate(level, Device.Frame.BranchCount, flags);
        Device.Frame.BranchCount++;

        Device.Frame.Track(_cmd);
        _vk.BeginCommandBuffer(_cmd, &beginInfo);
    }

    public override GpuCommandList End()
    {
        base.End();

        _vk.EndCommandBuffer(_cmd);

        if (_cmd.Flags.Has(GpuCommandListFlags.Deferred))
            return _cmd;

        // Use empty fence handle if the CPU doesn't need to wait for the command list to finish.
        Fence fence = new Fence();
        if (_cmd.Fence != null)
            fence = _cmd.Fence.Handle;

        // Submit command list and don't return the command list, as it's not deferred.
        SubmitCommandList(_cmd, fence);
        return null;
    }

    /// <inheritdoc/>
    public override unsafe void Execute(GpuCommandList list)
    {
        CommandListVK cmd = list as CommandListVK;
        if (cmd.Level != CommandBufferLevel.Secondary)
            throw new InvalidOperationException("Cannot submit a secondary command list to a queue");

        /*CommandListVK vkList = list as CommandListVK;

        CommandBuffer* cmdBuffers = stackalloc CommandBuffer[1] { vkList.Ptr };
        _vk.CmdExecuteCommands(_cmd, 1, cmdBuffers);*/

        CommandBuffer* ptrBuffers = stackalloc CommandBuffer[] { cmd.Ptr };
        SubmitInfo submit = new SubmitInfo(StructureType.SubmitInfo);
        submit.PCommandBuffers = ptrBuffers;

        _lockerExecute.Lock();

        // We want to wait on the previous command list's semaphore before executing this one, if any.
        if (_prevCmdList != null)
        {
            Semaphore* waitSemaphores = stackalloc Semaphore[] { _prevCmdList.Semaphore.Handle };
            submit.WaitSemaphoreCount = 1;
            submit.PWaitSemaphores = waitSemaphores;
        }
        else
        {
            submit.WaitSemaphoreCount = 0;
            submit.PWaitSemaphores = null;
        }

        // We want to signal the command list's own semaphore so that the next command list can wait on it, if needed.
        cmd.Semaphore.Reset();
        Semaphore* semaphore = stackalloc Semaphore[] { cmd.Semaphore.Handle };
        submit.CommandBufferCount = 1;
        submit.SignalSemaphoreCount = 1;
        submit.PSignalSemaphores = semaphore;

        _prevCmdList = cmd;
        Result r = VK.QueueSubmit(Native, 1, &submit, cmd.Fence.Handle);
        _lockerExecute.Unlock();
        r.Throw(_device, () => "Failed to submit command list");
    }

    /// <inheritdoc/>
    public override unsafe void Sync(GpuCommandListFlags flags = GpuCommandListFlags.None)
    {
        if (_cmd.Level != CommandBufferLevel.Primary)
        {
            throw new InvalidOperationException($"Cannot submit a secondary command list directly to a command queue.");
        }
        else
        {
            if (flags.Has(GpuCommandListFlags.Deferred))
                throw new InvalidOperationException($"An immediate/primary command list branch cannot use deferred flag during Sync() calls.");
        }

        // Use empty fence handle if the CPU doesn't need to wait for the command list to finish.
        Fence fence = new Fence();
        if (_cmd.Fence != null)
            fence = _cmd.Fence.Handle;

        // We're only submitting the current command buffer.
        _vk.EndCommandBuffer(_cmd);
        SubmitCommandList(_cmd, fence);

        // Allocate next command buffer
        _cmd = _poolFrame.Allocate(_cmd.Level, _cmd.BranchIndex, flags);
        CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
        beginInfo.Flags = CommandBufferUsageFlags.OneTimeSubmitBit;

        _vk.BeginCommandBuffer(_cmd, &beginInfo);
        Device.Frame.Track(_cmd);
    }


    internal bool HasFlags(CommandSetCapabilityFlags flags)
    {
        return (Flags & flags) == flags;
    }

    protected override void OnDispose(bool immediate)
    {
        _poolFrame.Dispose();
        _poolTransient.Dispose();
    }

    internal Vk VK => _vk;

    internal Logger Log { get; }

    /// <summary>
    /// Gets the Queue family index, in relation to the bound <see cref="DeviceVK"/>.
    /// </summary>
    internal uint FamilyIndex { get; }

    /// <summary>
    /// Gets the command queue index, within its family.
    /// </summary>
    internal uint Index { get; }

    /// <summary>
    /// Gets the underlying command set definition.
    /// </summary>
    internal SupportedCommandSet Set { get; }

    /// <summary>
    /// Gets flags representing the available API command sets.
    /// </summary>
    internal CommandSetCapabilityFlags Flags { get; }

    internal Queue Native { get; private set; }
}

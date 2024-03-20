using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Molten.Graphics.Vulkan;

public unsafe class CommandQueueVK : GpuObject<DeviceVK>
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
        Handle = queue;
        Set = set;

        _lockerExecute = new Interlocker();
        _poolFrame = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit, 1);
        _poolTransient = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit | CommandPoolCreateFlags.TransientBit, 5);
    }

    internal CommandListVK Allocate(GpuCommandListFlags flags)
    {
        CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
        beginInfo.Flags = CommandBufferUsageFlags.None;

        if (flags.Has(GpuCommandListFlags.SingleSubmit))
            beginInfo.Flags |= CommandBufferUsageFlags.OneTimeSubmitBit;

        CommandListVK cmd = _poolFrame.Allocate(CommandBufferLevel.Primary, Device.Frame.BranchCount, flags);
        return cmd;
    }

    /// <inheritdoc/>
    internal unsafe void Execute(GpuCommandList list)
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
        Result r = VK.QueueSubmit(Handle, 1, &submit, cmd.Fence.Handle);
        _lockerExecute.Unlock();
        r.Throw(_device, () => "Failed to submit command list");
    }

    internal bool HasFlags(CommandSetCapabilityFlags flags)
    {
        return (Flags & flags) == flags;
    }

    protected override void OnGraphicsRelease()
    {
        _poolFrame.Dispose(true);
        _poolTransient.Dispose(true);
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

    internal Queue Handle { get; private set; }
}

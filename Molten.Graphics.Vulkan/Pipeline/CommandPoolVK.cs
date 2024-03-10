using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal unsafe class CommandPoolVK : EngineObject
{
    CommandPool _pool;
    List<CommandPoolAllocation> _allocations;
    uint _allocSize;

    internal CommandPoolVK(GraphicsQueueVK queue, CommandPoolCreateFlags flags, uint allocationSize)
    {
        Queue = queue;
        _allocSize = allocationSize;

        CommandPoolCreateInfo info = new CommandPoolCreateInfo(StructureType.CommandPoolCreateInfo);
        info.Flags = flags;
        info.QueueFamilyIndex = queue.FamilyIndex;

        CommandPool pool = new CommandPool();
        Result r = Queue.VK.CreateCommandPool(Queue.Device, &info, null, &pool);
        if (!r.Check(Queue.Device, ()=> "Failed to create command buffer pool"))
            return;

        _pool = pool;
        IsTransient = (flags & CommandPoolCreateFlags.TransientBit) == CommandPoolCreateFlags.TransientBit;
        _allocations = new List<CommandPoolAllocation>();
    }

    internal CommandListVK Allocate(CommandBufferLevel level, uint branchIndex, GpuCommandListFlags flags)
    {
        CommandListVK result = null;
        foreach(CommandPoolAllocation a in _allocations)
        {
            if (a.Level != level)
                continue;

            CommandListVK list = a.Get();
            if (list != null)
            {
                Queue.VK.ResetCommandBuffer(list.Ptr, CommandBufferResetFlags.None);
                result = list;
                break;
            }
        }

        // If no free command buffers were found, allocate more before retrieving one.
        if (result == null)
        {
            CommandPoolAllocation allocation = new CommandPoolAllocation(this, level, _allocSize);
            _allocations.Add(allocation);
            result = allocation.Get();
        }

        if (flags.Has(GpuCommandListFlags.CpuSyncable))
        {
            if (result.Fence != null)
                result.Fence.Reset();
            else
                result.Fence = Queue.Device.GetFence();
        }
        else
        {
            if (result.Fence != null)
            {
                Queue.Device.FreeFence(result.Fence as FenceVK);
                result.Fence = null;
            }
        }

        result.Flags = flags;
        result.BranchIndex = branchIndex;
        return result;
    }

    protected override void OnDispose(bool immediate)
    {
        Queue.VK.DestroyCommandPool(Queue.Device, _pool, null);
    }

    internal GraphicsQueueVK Queue { get; }

    internal CommandPool Native => _pool;

    /// <summary>
    /// Gets whether or not the current <see cref="CommandPoolVK"/> products transient (short-lived) command buffers. 
    /// <para>Transient buffers will automatically free themselves once submitted to their parent queue.</para>
    /// </summary>
    internal bool IsTransient { get; }

    internal GpuCommandListFlags ListType { get; }
}

using Silk.NET.Vulkan;

namespace Molten.Graphics
{
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
            Result r = Queue.VK.CreateCommandPool(Queue.VKDevice, &info, null, &pool);
            if (!r.Check(Queue.VKDevice, ()=> "Failed to create command buffer pool"))
                return;

            IsTransient = (flags & CommandPoolCreateFlags.TransientBit) == CommandPoolCreateFlags.TransientBit;
            _allocations = new List<CommandPoolAllocation>();
        }

        internal CommandListVK Allocate(CommandBufferLevel level, uint branchIndex, uint listIndex, GraphicsCommandListFlags flags)
        {
            CommandListVK result = null;
            foreach(CommandPoolAllocation a in _allocations)
            {
                if (a.Level != level)
                    continue;

                CommandListVK list = a.Get();
                if (list != null)
                {
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

            if (flags.Has(GraphicsCommandListFlags.CpuSyncable))
                result.Fence = Queue.VKDevice.GetFence();
            else
                result.Fence = null;

            result.BranchIndex = branchIndex;
            result.Index = listIndex;
            return result;
        }

        protected override void OnDispose()
        {
            Queue.VK.DestroyCommandPool(Queue.VKDevice, _pool, null);
        }

        internal GraphicsQueueVK Queue { get; }

        internal CommandPool Native => _pool;

        /// <summary>
        /// Gets whether or not the current <see cref="CommandPoolVK"/> products transient (short-lived) command buffers. 
        /// <para>Transient buffers will automatically free themselves once submitted to their parent queue.</para>
        /// </summary>
        internal bool IsTransient { get; }

        internal GraphicsCommandListFlags ListType { get; }
    }
}

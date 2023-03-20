using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class CommandPoolVK : EngineObject
    {
        CommandPool _pool;
        List<CommandPoolAllocation> _allocations;
        uint _allocSize;

        internal CommandPoolVK(CommandQueueVK queue, CommandPoolCreateFlags flags, uint allocationSize)
        {
            Device = queue.VKDevice;
            _allocSize = allocationSize;


            CommandPoolCreateInfo info = new CommandPoolCreateInfo(StructureType.CommandPoolCreateInfo);
            info.Flags = flags;
            info.QueueFamilyIndex = queue.FamilyIndex;

            CommandPool pool = new CommandPool();
            Result r = Device.VK.CreateCommandPool(Device, &info, null, &pool);
            if (!r.Check(Device, ()=> "Failed to create command buffer pool"))
                return;

            IsTransient = (flags & CommandPoolCreateFlags.TransientBit) == CommandPoolCreateFlags.TransientBit;
            _allocations = new List<CommandPoolAllocation>();
        }

        internal CommandListVK Allocate(CommandBufferLevel level)
        {
            CommandListVK list = null;

            foreach(CommandPoolAllocation a in _allocations)
            {
                if (a.Level != level)
                    continue;

                list = a.Get();
                if (list != null)
                    return list;
            }

            CommandPoolAllocation allocation = new CommandPoolAllocation(this, level, _allocSize);
            _allocations.Add(allocation);
            return allocation.Get();
        }

        protected override void OnDispose()
        {
            Device.VK.DestroyCommandPool(Device, _pool, null);
        }

        internal DeviceVK Device { get; }

        internal CommandPool Native => _pool;

        /// <summary>
        /// Gets whether or not the current <see cref="CommandPoolVK"/> products transient (short-lived) command buffers. 
        /// <para>Transient buffers will automatically free themselves once submitted to their parent queue.</para>
        /// </summary>
        internal bool IsTransient { get; }
    }
}

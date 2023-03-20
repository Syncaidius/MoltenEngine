using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class CommandPoolAllocation : IDisposable
    {
        CommandBuffer* _ptrBuffers;
        CommandListVK[] _lists;
        CommandListVK[] _free;
        uint _freeCount;

        internal CommandPoolAllocation(CommandPoolVK pool, CommandBufferLevel level, uint count)
        {
            Level = level;
            _freeCount = count;
            Pool = pool;

            CommandBufferAllocateInfo info = new CommandBufferAllocateInfo(StructureType.CommandBufferAllocateInfo);
            info.Level = level;
            info.CommandPool = pool.Native;
            info.CommandBufferCount = 1;

            CommandBuffer* cbs = EngineUtil.AllocArray<CommandBuffer>(count);
            Result r = pool.Device.VK.AllocateCommandBuffers(pool.Device, null, cbs);
            if (!r.Check(pool.Device, () => "Failed to allocate command buffers"))
                _freeCount = 0;

            _free = new CommandListVK[count];
            _lists = new CommandListVK[count];
            for (int i = 0; i < _lists.Length; i++)
            {
                _lists[i] = new CommandListVK(this, _ptrBuffers[i]);
                _free[i] = _lists[i];
            }
        }

        internal CommandListVK Get()
        {
            if (_freeCount == 0)
                return null;

            return _free[--_freeCount];
        }

        internal void Free(CommandListVK list)
        {
            _free[_freeCount++] = list;
        }

        public void Dispose()
        {
            _freeCount = 0;

            Pool.Device.VK.FreeCommandBuffers(Pool.Device, Pool.Native, (uint)_lists.Length, _ptrBuffers);
            EngineUtil.Free(ref _ptrBuffers);
        }

        internal ref CommandBuffer* Ptr => ref _ptrBuffers;

        /// <summary>
        /// Gets the parent pool that owns the current <see cref="CommandPoolAllocation"/>
        /// </summary>
        internal CommandPoolVK Pool { get; }

        internal CommandBufferLevel Level { get; }
    }
}

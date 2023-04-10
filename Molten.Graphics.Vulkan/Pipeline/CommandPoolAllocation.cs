using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
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
            Result r = pool.Queue.VK.AllocateCommandBuffers(pool.Queue.VKDevice, null, cbs);
            if (!r.Check(pool.Queue.VKDevice, () => "Failed to allocate command buffers"))
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

            CommandListVK list = _free[--_freeCount];
            list.IsFree = false;
            return list;
        }

        internal void Free(CommandListVK list)
        {
            _free[_freeCount++] = list;
        }

        public void Dispose()
        {
            _freeCount = 0;

            Pool.Queue.VK.FreeCommandBuffers(Pool.Queue.VKDevice, Pool.Native, (uint)_lists.Length, _ptrBuffers);
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

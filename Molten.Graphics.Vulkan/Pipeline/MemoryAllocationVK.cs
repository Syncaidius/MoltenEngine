using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class MemoryAllocationVK
    {
        MemoryHeapVK _heap;
        DeviceMemory _memory;

        internal MemoryAllocationVK(MemoryHeapVK heap, ulong numBytes, ref DeviceMemory memory)
        {
            _heap = heap;
            Size = numBytes;
            _memory = memory;
        }

        internal void Free()
        {
            _heap.Free(this);
        }

        public static implicit operator DeviceMemory(MemoryAllocationVK allocation)
        {
            return allocation._memory;
        }

        internal ref DeviceMemory Handle => ref _memory;

        internal ulong Size { get; }
    }
}

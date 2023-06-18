using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class MemoryHeapVK
    {
        internal struct TypeIndex
        {
            internal MemoryType Type;

            internal uint Index;

            internal TypeIndex(MemoryType type, uint index)
            {
                Type = type;
                Index = index;
            }
        }

        List<MemoryAllocationVK> _allocations;
        DeviceVK _device;
        TypeIndex[] _memTypes;

        internal MemoryHeapVK(DeviceVK device, MemoryHeap heap, uint index, TypeIndex[] types)
        {
            _device = device;
            _allocations = new List<MemoryAllocationVK>();
            _memTypes = types;
            Size = heap.Size;

            Index = index;
            Size = heap.Size;
            Flags = heap.Flags;
        }

        internal unsafe MemoryAllocationVK Allocate(ref MemoryRequirements requirements, MemoryPropertyFlags flags)
        {
            for(int i = 0; i < _memTypes.Length; i++)
            {
                ref TypeIndex mt = ref _memTypes[i];

                if ((requirements.MemoryTypeBits & (1U << i)) == (1U << i) && (mt.Type.PropertyFlags & flags) == flags)
                {
                    MemoryAllocateInfo memInfo = new MemoryAllocateInfo();
                    memInfo.SType = StructureType.MemoryAllocateInfo;
                    memInfo.AllocationSize = requirements.Size;
                    memInfo.MemoryTypeIndex = _memTypes[i].Index;

                    DeviceMemory memory = new DeviceMemory();
                    Result r = _device.VK.AllocateMemory(_device, &memInfo, null, &memory);
                    if (!r.Check(_device))
                        return null;

                    MemoryAllocationVK mem = new MemoryAllocationVK(this, requirements.Size, ref memory, flags);
                    Allocated += requirements.Size;
                    _allocations.Add(mem);
                    return mem;
                }
            }

            return null;
        }

        internal unsafe void Free(MemoryAllocationVK allocation)
        {
            _device.VK.FreeMemory(_device, allocation.Handle, null);
            Allocated -= allocation.Size;
        }

        public bool HasFlags(MemoryHeapFlags flags)
        {
            return (Flags & flags) == flags;
        }

        /// <summary>
        /// Gets the capacity of the current <see cref="MemoryHeapVK"/>, in bytes.
        /// </summary>
        internal ulong Size { get; }

        internal ulong Allocated { get; private set; }

        /// <summary>
        /// Gets the vulkan memory heap index.
        /// </summary>
        internal uint Index { get; }

        internal MemoryHeapFlags Flags { get; }
    }
}

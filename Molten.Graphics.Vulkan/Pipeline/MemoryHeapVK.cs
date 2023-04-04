using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class MemoryHeapVK
    {
        List<MemoryAllocationVK> _allocations;
        bool _isDeviceMemory;
        DeviceVK _device;
        uint[] _memTypeIndices;
        MemoryType[] _memTypes;

        internal MemoryHeapVK(DeviceVK device, MemoryHeap heap, uint index, uint[] memTypeIndices)
        {
            _device = device;
            _allocations = new List<MemoryAllocationVK>();
            _memTypeIndices = memTypeIndices;

            _memTypes = new MemoryType[memTypeIndices.Length];
            for(int i = 0; i < memTypeIndices.Length; i++)
                _memTypes[i] = device.Memory.MemoryTypes[(int)_memTypeIndices[i]];

            _isDeviceMemory = (Flags & MemoryHeapFlags.DeviceLocalBit) == MemoryHeapFlags.DeviceLocalBit;

            Index = index;
            Capacity = heap.Size;
            Flags = heap.Flags;
        }

        internal unsafe MemoryAllocationVK Allocate(ulong numBytes, MemoryPropertyFlags flags)
        {
            for(int i = 0; i < _memTypes.Length; i++)
            {
                if(_memTypes[i].PropertyFlags == flags)
                {
                    MemoryAllocateInfo memInfo = new MemoryAllocateInfo();
                    memInfo.SType = StructureType.MemoryAllocateInfo;
                    memInfo.AllocationSize = numBytes;
                    memInfo.MemoryTypeIndex = _memTypeIndices[i];

                    DeviceMemory memory = new DeviceMemory();
                    Result r = _device.VK.AllocateMemory(_device, &memInfo, null, &memory);
                    if (!r.Check(_device))
                        return null;

                    MemoryAllocationVK mem = new MemoryAllocationVK(this, numBytes, ref memory);
                    _allocations.Add(mem);
                    return mem;
                }
            }

            _device.Log.Error($"Unable to allocate {numBytes:N0} bytes in heap {Index} [{Flags}] using allocation flags [{flags}].");
            return null;
        }

        internal unsafe void Free(MemoryAllocationVK allocation)
        {
            _device.VK.FreeMemory(_device, allocation.Handle, null);
        }

        /// <summary>
        /// Gets the capacity of the current <see cref="MemoryHeapVK"/>, in bytes.
        /// </summary>
        internal ulong Capacity { get; }

        internal ulong Allocated { get; }

        /// <summary>
        /// Gets the vulkan memory heap index.
        /// </summary>
        internal uint Index { get; }

        internal MemoryHeapFlags Flags { get; }
    }
}

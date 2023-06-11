using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class MemoryManagerVK
    {
        MemoryHeapVK[] _heaps;
        DeviceVK _device;

        internal unsafe MemoryManagerVK(DeviceVK device)
        {
            _device = device;
            PhysicalDeviceMemoryProperties p;
            RendererVK rendererVK = device.Renderer as RendererVK;
            if (rendererVK.ApiVersion < new VersionVK(1, 1))
            {
                p = device.VK.GetPhysicalDeviceMemoryProperties(device.Adapter);
            }
            else
            {
                PhysicalDeviceMemoryProperties2 mem = new PhysicalDeviceMemoryProperties2(StructureType.PhysicalDeviceMemoryProperties2);
                device.VK.GetPhysicalDeviceMemoryProperties2(device.Adapter, &mem);
                p = mem.MemoryProperties;
            }

            List<MemoryHeapVK.TypeIndex> types = new List<MemoryHeapVK.TypeIndex>();
            _heaps = new MemoryHeapVK[p.MemoryHeapCount];

            for (uint i = 0; i < p.MemoryHeapCount; i++)
            {
                ref MemoryHeap mh = ref p.MemoryHeaps[(int)i];

                // Check for memory types that are assignable to the current heap.
                for (uint j = 0; j < p.MemoryTypeCount; j++)
                {
                    ref MemoryType mt = ref p.MemoryTypes[(int)j];
                    if (mt.HeapIndex == i)
                        types.Add(new MemoryHeapVK.TypeIndex(mt, j));
                }

                _heaps[i] = new MemoryHeapVK(device, mh, i, types.ToArray());
                types.Clear();
            }
        }

        internal MemoryAllocationVK Allocate(ref MemoryRequirements requirements, MemoryPropertyFlags flags)
        {
            for (int i = 0; i < _heaps.Length; i++)
            {
                // TODO Prioritize host-visible if the device supports ReBAR, as this allows direct CPU access to avoid copy/transfers.

                MemoryAllocationVK mem = _heaps[i].Allocate(ref requirements, flags);
                if (mem != null)
                    return mem;
            }

            _device.Log.Error($"Unable to find heap to allocate {requirements.Size:N0} bytes using allocation flags [{flags}].");
            return null;
        }

        internal int HeapCount => _heaps.Length;

        internal MemoryHeapVK this[uint heapIndex] => _heaps[heapIndex];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics
{
    internal unsafe abstract class BufferVK : GraphicsObject<Buffer> //, IGraphicsBuffer
    {
        Buffer* _buffer;
        BufferCreateInfo _desc;
        DeviceMemory* _memory;

        protected BufferVK(GraphicsDevice device,
            BufferFlags bufferFlags,
            BufferUsageFlags usageFlags,
            uint stride,
            uint numElements,
            void* initialData = null) :
            base(device,
                ((usageFlags & BufferUsageFlags.StorageBufferBit) == BufferUsageFlags.StorageBufferBit ? GraphicsBindTypeFlags.Output : GraphicsBindTypeFlags.None) |
                (
                    (usageFlags & BufferUsageFlags.TransferSrcBit) == BufferUsageFlags.TransferSrcBit ||
                    (usageFlags & BufferUsageFlags.VertexBufferBit) == BufferUsageFlags.VertexBufferBit || 
                    (usageFlags & BufferUsageFlags.IndexBufferBit) == BufferUsageFlags.IndexBufferBit || 
                    (usageFlags & BufferUsageFlags.UniformBufferBit) == BufferUsageFlags.UniformBufferBit ||
                    (usageFlags & BufferUsageFlags.UniformTexelBufferBit) == BufferUsageFlags.UniformTexelBufferBit
                    ? GraphicsBindTypeFlags.Input : GraphicsBindTypeFlags.None)
                )
        {
            Flags = bufferFlags;
            Stride = stride;
            ByteCapacity = Stride * numElements;
            ElementCount = numElements;

            MemoryPropertyFlags memFlags = BuildDescription(bufferFlags, usageFlags);
            InitializeBuffer(memFlags, initialData);
        }

        private MemoryPropertyFlags BuildDescription(BufferFlags bufferFlags, BufferUsageFlags usage)
        {
            MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;

            // Does the memory need to be host-visible?
            if(bufferFlags.HasFlags(BufferFlags.CpuRead) || bufferFlags.HasFlags(BufferFlags.CpuWrite))
                memFlags |= MemoryPropertyFlags.HostCoherentBit | MemoryPropertyFlags.HostVisibleBit;
            else
                memFlags |= MemoryPropertyFlags.DeviceLocalBit;

            if (bufferFlags.HasFlags(BufferFlags.GpuRead))
                usage |= BufferUsageFlags.TransferSrcBit;

            if (bufferFlags.HasFlags(BufferFlags.GpuWrite))
                usage |= BufferUsageFlags.TransferDstBit;

            _desc.SType = StructureType.BufferCreateInfo;
            _desc.Usage = usage;
            _desc.SharingMode = SharingMode.Exclusive;
            _desc.Flags = BufferCreateFlags.None;
            _desc.Size = Stride * ElementCount;

            return memFlags;
        }

        private void InitializeBuffer(MemoryPropertyFlags memFlags, void* initialData)
        {
            _buffer = EngineUtil.Alloc<Buffer>();
            _memory = EngineUtil.Alloc<DeviceMemory>();

            DeviceVK device = Device as DeviceVK;
            Result r = device.VK.CreateBuffer(device, in _desc, null, _buffer);
            if (!device.Renderer.CheckResult(r))
                return;

            MemoryRequirements memRequirements;
            device.VK.GetBufferMemoryRequirements(device, *_buffer, &memRequirements);

            MemoryAllocateInfo memInfo = new MemoryAllocateInfo();
            memInfo.SType = StructureType.MemoryAllocateInfo;
            memInfo.AllocationSize = memRequirements.Size;
            memInfo.MemoryTypeIndex = device.Adapter.GetMemoryTypeIndex(ref memRequirements, memFlags);


            r = device.VK.AllocateMemory(device, &memInfo, null, _memory);
            if (!device.Renderer.CheckResult(r))
                return;

            device.VK.BindBufferMemory(device, *_buffer, *_memory, 0);
        }

        public override void GraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;

            if (_buffer != null)
            {
                device.VK.DestroyBuffer(device, *_buffer, null);
                device.VK.FreeMemory(device, *_memory, null);

                EngineUtil.Free(ref _memory);
                EngineUtil.Free(ref _buffer);
            }
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            throw new NotImplementedException();
        }

        public override unsafe Buffer* NativePtr => _buffer;

        /// <summary>Gets the stride (byte size) of each element within the current <see cref="BufferDX11"/>.</summary>
        public uint Stride { get; }

        /// <summary>Gets the capacity of a single section within the buffer, in bytes.</summary>
        public uint ByteCapacity { get; }

        /// <summary>
        /// Gets the number of elements that the current <see cref="BufferDX11"/> can store.
        /// </summary>
        public uint ElementCount { get; }

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        public BufferFlags Flags { get; }
    }
}

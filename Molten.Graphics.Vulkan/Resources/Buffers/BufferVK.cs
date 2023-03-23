using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.IO;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics
{
    internal unsafe abstract class BufferVK : ResourceVK, IGraphicsBuffer
    {
        Buffer* _buffer;
        BufferCreateInfo _desc;
        DeviceMemory* _memory;

        protected BufferVK(GraphicsDevice device,
            GraphicsResourceFlags bufferFlags,
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
            SizeInBytes = Stride * numElements;
            ElementCount = numElements;

            MemoryPropertyFlags memFlags = BuildDescription(bufferFlags, usageFlags);
            InitializeBuffer(memFlags, initialData);
        }

        private MemoryPropertyFlags BuildDescription(GraphicsResourceFlags bufferFlags, BufferUsageFlags usage)
        {
            MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;

            // Does the memory need to be host-visible?
            if(bufferFlags.Has(GraphicsResourceFlags.CpuRead) || bufferFlags.Has(GraphicsResourceFlags.CpuWrite))
                memFlags |= MemoryPropertyFlags.HostCoherentBit | MemoryPropertyFlags.HostVisibleBit;
            else
                memFlags |= MemoryPropertyFlags.DeviceLocalBit;

            if (bufferFlags.Has(GraphicsResourceFlags.None))
                usage |= BufferUsageFlags.TransferSrcBit;

            if (bufferFlags.Has(GraphicsResourceFlags.GpuWrite))
                usage |= BufferUsageFlags.TransferDstBit;

            _desc.SType = StructureType.BufferCreateInfo;
            _desc.Usage = usage;
            _desc.SharingMode = SharingMode.Exclusive;
            _desc.Flags = BufferCreateFlags.None;
            _desc.Size = Stride * ElementCount;
            _desc.PQueueFamilyIndices = EngineUtil.AllocArray<uint>(1);
            _desc.PQueueFamilyIndices[0] = (Device.Cmd as CommandQueueVK).Index;
            _desc.QueueFamilyIndexCount = 1;

            return memFlags;
        }

        private void InitializeBuffer(MemoryPropertyFlags memFlags, void* initialData)
        {
            _buffer = EngineUtil.Alloc<Buffer>();
            _memory = EngineUtil.Alloc<DeviceMemory>();

            DeviceVK device = Device as DeviceVK;
            Result r = device.VK.CreateBuffer(device, in _desc, null, _buffer);
            if (!r.Check(device))
                return;

            MemoryRequirements memRequirements;
            device.VK.GetBufferMemoryRequirements(device, *_buffer, &memRequirements);

            MemoryAllocateInfo memInfo = new MemoryAllocateInfo();
            memInfo.SType = StructureType.MemoryAllocateInfo;
            memInfo.AllocationSize = memRequirements.Size;
            memInfo.MemoryTypeIndex = device.Adapter.GetMemoryTypeIndex(ref memRequirements, memFlags);

            r = device.VK.AllocateMemory(device, &memInfo, null, _memory);
            if (!r.Check(device))
                return;

            r = device.VK.BindBufferMemory(device, *_buffer, *_memory, 0);
            if (!r.Check(device))
                return;
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
                EngineUtil.Free(ref _desc.PQueueFamilyIndices);
            }
        }

        public void SetData<T>(GraphicsPriority priority, T[] data, IStagingBuffer staging = null, Action completeCallback = null) 
            where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint elementCount, uint byteOffset = 0, IStagingBuffer staging = null,
            Action completeCallback = null) 
            where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint elementOffset, 
            Action<T[]> completionCallback = null) 
            where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, Action<GraphicsResource> completionCallback = null)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(GraphicsPriority priority, IGraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0, 
            Action<GraphicsResource> completionCallback = null)
        {
            throw new NotImplementedException();
        }

        public void GetStream(GraphicsPriority priority, Action<IGraphicsBuffer, RawStream> callback, IStagingBuffer staging = null)
        {
            throw new NotImplementedException();
        }

        internal bool HasFlags(BufferUsageFlags flags)
        {
            return (_desc.Usage & flags) == flags;
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }

        internal unsafe Buffer* NativePtr => _buffer;

        internal override DeviceMemory* Memory => _memory;

        /// <summary>Gets the stride (byte size) of each element within the current <see cref="BufferDX11"/>.</summary>
        public uint Stride { get; }

        /// <summary>Gets the capacity of a single section within the buffer, in bytes.</summary>
        public override uint SizeInBytes { get; }

        /// <summary>
        /// Gets the number of elements that the current <see cref="BufferDX11"/> can store.
        /// </summary>
        public uint ElementCount { get; }

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        public override GraphicsResourceFlags Flags { get; }

        public override bool IsUnorderedAccess => HasFlags(BufferUsageFlags.StorageBufferBit);
    }
}

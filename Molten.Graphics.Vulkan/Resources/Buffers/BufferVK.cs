using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics.Vulkan
{
    public unsafe class BufferVK : GraphicsBuffer
    {
        BufferCreateInfo _desc;
        ResourceHandleVK* _handle;
        MemoryAllocationVK _memory;

        internal BufferVK(GraphicsDevice device,
            GraphicsBufferType type,
            GraphicsResourceFlags flags,
            uint stride,
            uint numElements) :
            base(device, stride, numElements, flags, type)
        {
            _handle = ResourceHandleVK.AllocateNew<Buffer>();

            BufferUsageFlags usageFlags = BufferUsageFlags.None;

            if (Flags.Has(GraphicsResourceFlags.None))
                usageFlags |= BufferUsageFlags.TransferSrcBit;

            if (Flags.Has(GraphicsResourceFlags.GpuWrite))
                usageFlags |= BufferUsageFlags.TransferDstBit;

            switch (type)
            {
                case GraphicsBufferType.Vertex:
                    usageFlags |= BufferUsageFlags.VertexBufferBit;
                    break;

                case GraphicsBufferType.Index:
                    usageFlags |= BufferUsageFlags.IndexBufferBit;
                    break;

                case GraphicsBufferType.Staging: // Staging buffers always require CPU write access.
                    flags |= GraphicsResourceFlags.CpuWrite;
                    break;

                case GraphicsBufferType.Constant:
                    usageFlags |= BufferUsageFlags.UniformBufferBit;
                    break;
            }

            InitializeBuffer(usageFlags);
        }

        private void InitializeBuffer(BufferUsageFlags usage)
        {
            MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;

            // Does the memory need to be host-visible?
            if (Flags.Has(GraphicsResourceFlags.CpuRead) || Flags.Has(GraphicsResourceFlags.CpuWrite))
            {
                // In Vulkan, the CPU either has read AND write access, or none at all.
                // If either of the CPU access flags were provided, we need to add both.
                Flags |= GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.CpuWrite;
                memFlags |= MemoryPropertyFlags.HostCoherentBit | MemoryPropertyFlags.HostVisibleBit;
            }
            else
            {
                memFlags |= MemoryPropertyFlags.DeviceLocalBit;
            }

            _desc.SType = StructureType.BufferCreateInfo;
            _desc.Usage = usage;
            _desc.SharingMode = SharingMode.Exclusive;
            _desc.Flags = BufferCreateFlags.None;
            _desc.Size = Stride * ElementCount;
            _desc.PQueueFamilyIndices = EngineUtil.AllocArray<uint>(1);
            _desc.PQueueFamilyIndices[0] = (Device.Queue as GraphicsQueueVK).Index;
            _desc.QueueFamilyIndexCount = 1;

            DeviceVK device = Device as DeviceVK;
            Result r = device.VK.CreateBuffer(device, in _desc, null, (Buffer*)_handle->Ptr);
            if (!r.Check(device))
                return;

            MemoryRequirements memRequirements;
            device.VK.GetBufferMemoryRequirements(device, *(Buffer*)_handle->Ptr, &memRequirements);
            _memory = device.Memory.Allocate(ref memRequirements, memFlags);
            if (_memory == null)
                throw new GraphicsResourceException(this, "Unable to allocate memory for buffer.");

            _handle->Memory = _memory;
            _handle->MemoryFlags = memFlags;
            r = device.VK.BindBufferMemory(device, *(Buffer*)_handle->Ptr, _handle->Memory, 0);
            if (!r.Check(device))
                return;
        }

        protected override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;

            if (_handle->Ptr != null)
            {
                device.VK.DestroyBuffer(device, *(Buffer*)_handle->Ptr, null);
                device.VK.FreeMemory(device, _handle->Memory, null);

                _handle->Dispose();
                EngineUtil.Free(ref _handle);
                EngineUtil.Free(ref _desc.PQueueFamilyIndices);
            }
        }

        protected override void OnApply(GraphicsQueue queue) { }

        public override unsafe void* Handle => _handle;

        public override unsafe void* UAV => throw new NotImplementedException();

        public override unsafe void* SRV => throw new NotImplementedException();

        public override GraphicsFormat ResourceFormat { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    }
}

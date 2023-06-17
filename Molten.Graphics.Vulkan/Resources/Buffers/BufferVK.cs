using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe class BufferVK : GraphicsBuffer
    {
        BufferCreateInfo _desc;
        BufferHandleVK[] _handles;
        BufferHandleVK _curHandle;
        MemoryAllocationVK _memory;

        internal BufferVK(GraphicsDevice device,
            GraphicsBufferType type,
            GraphicsResourceFlags flags,
            uint stride,
            uint numElements) :
            base(device, stride, numElements, flags, type)
        { }

        protected override void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID)
        {
            _curHandle = _handles[frameBufferIndex];
        }

        protected override void CreateResource(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            DeviceVK device = Device as DeviceVK;

            // Check that we don't already have an image handle.
            if (_curHandle == null)
            {
                _handles = new BufferHandleVK[frameBufferSize];

                for (uint i = 0; i < frameBufferSize; i++)
                    _handles[i] = new BufferHandleVK(device);

                _curHandle = _handles[frameBufferIndex];
                BufferUsageFlags usageFlags = BufferUsageFlags.None;
                MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;

                if (Flags.Has(GraphicsResourceFlags.None))
                    usageFlags |= BufferUsageFlags.TransferSrcBit;

                if (Flags.Has(GraphicsResourceFlags.GpuWrite))
                    usageFlags |= BufferUsageFlags.TransferDstBit;

                // Check if any extra flags need to be enforced based on buffer type.
                switch (BufferType)
                {
                    case GraphicsBufferType.Vertex:
                        usageFlags |= BufferUsageFlags.VertexBufferBit;
                        break;

                    case GraphicsBufferType.Index:
                        usageFlags |= BufferUsageFlags.IndexBufferBit;
                        break;

                    case GraphicsBufferType.Staging: // Staging buffers always require CPU write access.
                        Flags |= GraphicsResourceFlags.CpuWrite;
                        break;

                    case GraphicsBufferType.Constant:
                        usageFlags |= BufferUsageFlags.UniformBufferBit;
                        break;
                }

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
                _desc.Usage = usageFlags;
                _desc.SharingMode = SharingMode.Exclusive;
                _desc.Flags = BufferCreateFlags.None;
                _desc.Size = Stride * ElementCount;
                _desc.PQueueFamilyIndices = EngineUtil.AllocArray<uint>(1);
                _desc.PQueueFamilyIndices[0] = (Device.Queue as GraphicsQueueVK).Index;
                _desc.QueueFamilyIndexCount = 1;
            }
            else if (lastFrameBufferSize != frameBufferIndex)
            {
                throw new NotImplementedException();
            }
        }

        private void CreateBuffer(DeviceVK device, BufferHandleVK handle, MemoryPropertyFlags memFlags)
        {
            Result r = device.VK.CreateBuffer(device, in _desc, null, handle.NativePtr);
            if (!r.Check(device))
                return;

            MemoryRequirements memRequirements;
            device.VK.GetBufferMemoryRequirements(device, *handle.NativePtr, &memRequirements);
            handle.Memory = device.Memory.Allocate(ref memRequirements, handle.MemoryFlags);
            if (handle.Memory == null)
                throw new GraphicsResourceException(this, "Unable to allocate memory for buffer.");

            handle.MemoryFlags = memFlags;
            r = device.VK.BindBufferMemory(device, *handle.NativePtr, handle.Memory, 0);
            if (!r.Check(device))
                return;
        }

        protected override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;
            for (int i = 0; i < KnownFrameBufferSize; i++)
            {
                if (_handles[i].ViewPtr != null)
                    device.VK.DestroyBufferView(device, *_handles[i].ViewPtr, null);

                if (_handles[i].NativePtr != null)
                    device.VK.DestroyBuffer(device, *_handles[i].NativePtr, null);

                _handles[i].Dispose();
            }
        }

        public override unsafe BufferHandleVK Handle => _curHandle;

        public override unsafe void* UAV => throw new NotImplementedException();

        public override unsafe void* SRV => _curHandle.ViewPtr;

        public override GraphicsFormat ResourceFormat { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    }
}

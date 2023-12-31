using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics.Vulkan
{
    public unsafe class BufferVK : GraphicsBuffer
    {
        BufferCreateInfo _desc;
        ResourceHandleVK<Buffer, BufferHandleVK>[] _handles;
        ResourceHandleVK<Buffer, BufferHandleVK> _curHandle;
        MemoryAllocationVK _memory;

        internal BufferVK(GraphicsDevice device,
            GraphicsBufferType type,
            GraphicsResourceFlags flags,
            uint stride,
            uint numElements) :
            base(device, stride, numElements, flags, type)
        {
            ResourceFormat = GraphicsFormat.Unknown;
        }

        protected override void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            throw new NotImplementedException();
        }

        protected override void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID)
        {
            _curHandle = _handles[frameBufferIndex];
            _curHandle.UpdateUsage(frameID);
        }

        protected override void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID)
        {
            DeviceVK device = Device as DeviceVK;
            _handles = new ResourceHandleVK<Buffer, BufferHandleVK>[frameBufferSize];

            for (uint i = 0; i < frameBufferSize; i++)
                _handles[i] = new ResourceHandleVK<Buffer, BufferHandleVK>(this, true, CreateBuffer);

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

            foreach(ResourceHandleVK<Buffer, BufferHandleVK> handle in _handles)
                CreateBuffer(device, handle.SubHandle, memFlags);
        }

        private void CreateBuffer(DeviceVK device, BufferHandleVK subHandle, MemoryPropertyFlags memFlags)
        {
            Result r = device.VK.CreateBuffer(device, in _desc, null, subHandle.Ptr);
            if (!r.Check(device))
                return;

            MemoryRequirements memRequirements;
            device.VK.GetBufferMemoryRequirements(device, *subHandle.Ptr, &memRequirements);
            subHandle.Memory = device.Memory.Allocate(ref memRequirements, memFlags);
            if (subHandle.Memory == null)
                throw new GraphicsResourceException(this, "Unable to allocate memory for buffer.");

            r = device.VK.BindBufferMemory(device, *subHandle.Ptr, subHandle.Memory, 0);
            if (!r.Check(device))
                return;
        }

        protected override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;
            for (int i = 0; i < KnownFrameBufferSize; i++)
            {
                if (_handles[i].SubHandle.ViewPtr != null)
                    device.VK.DestroyBufferView(device, *_handles[i].SubHandle.ViewPtr, null);

                if (_handles[i].NativePtr != null)
                    device.VK.DestroyBuffer(device, *_handles[i].NativePtr, null);

                _handles[i].Dispose();
            }
        }

        public override unsafe ResourceHandleVK<Buffer, BufferHandleVK> Handle => _curHandle;

        public override GraphicsFormat ResourceFormat { get; protected set; }
    }
}

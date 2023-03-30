using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics
{
    internal unsafe abstract class BufferVK : GraphicsBuffer
    {
        BufferCreateInfo _desc;
        ResourceHandleVK* _handle;

        protected BufferVK(GraphicsDevice device,
            GraphicsBufferType type,
            GraphicsResourceFlags flags,
            BufferUsageFlags usageFlags,
            uint stride,
            uint numElements,
            void* initialData = null) :
            base(device, stride, numElements, flags, type)
        {
            _handle = EngineUtil.Alloc<ResourceHandleVK>();

            MemoryPropertyFlags memFlags = BuildDescription(flags, usageFlags);
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
            _handle->Ptr = EngineUtil.Alloc<Buffer>();
            _handle->Memory = EngineUtil.Alloc<DeviceMemory>();

            DeviceVK device = Device as DeviceVK;
            Result r = device.VK.CreateBuffer(device, in _desc, null, (Buffer*)_handle->Ptr);
            if (!r.Check(device))
                return;

            MemoryRequirements memRequirements;
            device.VK.GetBufferMemoryRequirements(device, *(Buffer*)_handle->Ptr, &memRequirements);

            MemoryAllocateInfo memInfo = new MemoryAllocateInfo();
            memInfo.SType = StructureType.MemoryAllocateInfo;
            memInfo.AllocationSize = memRequirements.Size;
            memInfo.MemoryTypeIndex = device.Adapter.GetMemoryTypeIndex(ref memRequirements, memFlags);

            r = device.VK.AllocateMemory(device, &memInfo, null, _handle->Memory);
            if (!r.Check(device))
                return;

            r = device.VK.BindBufferMemory(device, *(Buffer*)_handle->Ptr, *_handle->Memory, 0);
            if (!r.Check(device))
                return;
        }

        public override void GraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;

            if (_handle->Ptr != null)
            {
                device.VK.DestroyBuffer(device, *(Buffer*)_handle->Ptr, null);
                device.VK.FreeMemory(device, *_handle->Memory, null);

                EngineUtil.Free(ref _handle->Memory);
                EngineUtil.Free(ref _handle->Ptr);
                EngineUtil.Free(ref _handle);
                EngineUtil.Free(ref _desc.PQueueFamilyIndices);
            }
        }

        internal bool HasFlags(BufferUsageFlags flags)
        {
            return (_desc.Usage & flags) == flags;
        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }

        public override unsafe void* Handle => _handle;

        public override unsafe void* UAV => throw new NotImplementedException();

        public override unsafe void* SRV => throw new NotImplementedException();

        public override GraphicsFormat ResourceFormat { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    }
}

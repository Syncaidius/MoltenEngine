using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Molten.Graphics.Vulkan;

public unsafe class BufferVK : GpuBuffer
{
    BufferCreateInfo _desc;
    ResourceHandleVK<Buffer, BufferHandleVK> _handle;
    MemoryAllocationVK _memory;

    internal BufferVK(GpuDevice device,
        GpuBufferType type,
        GpuResourceFlags flags,
        uint stride,
        uint numElements,
        uint alignment) :
        base(device, stride, numElements, flags, type, alignment)
    {
        ResourceFormat = GpuResourceFormat.Unknown;
    }

    protected override GpuBuffer OnAllocateSubBuffer(ulong offset, uint stride, ulong numElements, GpuResourceFlags flags, GpuBufferType type, uint alignment)
    {
        throw new NotImplementedException();
    }

    protected override void OnCreateResource()
    {
        DeviceVK device = Device as DeviceVK;
        _handle = new ResourceHandleVK<Buffer, BufferHandleVK>(this, true, CreateBuffer);

        BufferUsageFlags usageFlags = BufferUsageFlags.None;
        MemoryPropertyFlags memFlags = MemoryPropertyFlags.None;

        if (Flags.Has(GpuResourceFlags.None))
            usageFlags |= BufferUsageFlags.TransferSrcBit;

        if (Flags.Has(GpuResourceFlags.GpuWrite))
            usageFlags |= BufferUsageFlags.TransferDstBit;

        // Check if any extra flags need to be enforced based on buffer type.
        switch (BufferType)
        {
            case GpuBufferType.Vertex:
                usageFlags |= BufferUsageFlags.VertexBufferBit;
                break;

            case GpuBufferType.Index:
                usageFlags |= BufferUsageFlags.IndexBufferBit;
                break;

            case GpuBufferType.Staging: // Staging buffers always require CPU write access.
                Flags |= GpuResourceFlags.CpuWrite;
                break;

            case GpuBufferType.Constant:
                usageFlags |= BufferUsageFlags.UniformBufferBit;
                break;
        }

        // Does the memory need to be host-visible?
        if (Flags.Has(GpuResourceFlags.CpuRead) || Flags.Has(GpuResourceFlags.CpuWrite))
        {
            // In Vulkan, the CPU either has read AND write access, or none at all.
            // If either of the CPU access flags were provided, we need to add both.
            Flags |= GpuResourceFlags.CpuRead | GpuResourceFlags.CpuWrite;
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
        _desc.PQueueFamilyIndices[0] = (Device.Queue as CommandQueueVK).Index;
        _desc.QueueFamilyIndexCount = 1;

        CreateBuffer(device, _handle.SubHandle, memFlags);
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
            throw new GpuResourceException(this, "Unable to allocate memory for buffer.");

        r = device.VK.BindBufferMemory(device, *subHandle.Ptr, subHandle.Memory, 0);
        if (!r.Check(device))
            return;
    }

    protected override void OnGraphicsRelease()
    {
        DeviceVK device = Device as DeviceVK;
        if (_handle.SubHandle.ViewPtr != null)
            device.VK.DestroyBufferView(device, *_handle.SubHandle.ViewPtr, null);

        if (_handle.NativePtr != null)
            device.VK.DestroyBuffer(device, *_handle.NativePtr, null);
    }

    public override unsafe ResourceHandleVK<Buffer, BufferHandleVK> Handle => _handle;

    public override GpuResourceFormat ResourceFormat { get; protected set; }
}

using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal unsafe class CommandListVK : GpuCommandList
{
    CommandPoolAllocation _allocation;
    CommandBuffer _native;
    Vk _vk;
    DeviceVK _device;

    internal CommandListVK(CommandPoolAllocation allocation, CommandBuffer cmdBuffer) : 
        base(allocation.Pool.Queue.Device)
    {
        _allocation = allocation;
        _native = cmdBuffer;
        _device = allocation.Pool.Queue.Device;
        _vk = allocation.Pool.Queue.VK;
        Semaphore = new SemaphoreVK(_device);
    }

    public override void Wait(ulong nsTimeout = ulong.MaxValue)
    {
        throw new NotImplementedException();
    }

    public override void Free()
    {
        if (IsFree)
            return;

        IsFree = true;
        _allocation.Free(this);
    }

    protected override void OnGraphicsRelease()
    {
        throw new NotImplementedException();
    }

    public static implicit operator CommandBuffer(CommandListVK list)
    {
        return list._native;
    }

    internal bool IsFree { get; set; }

    internal CommandBuffer Ptr => _native;

    internal CommandBufferLevel Level => _allocation.Level;

    internal SemaphoreVK Semaphore { get; }

    internal FenceVK Fence { get; set; }
}

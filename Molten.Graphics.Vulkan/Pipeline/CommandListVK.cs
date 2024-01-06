using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal unsafe class CommandListVK : GraphicsCommandList
{
    CommandPoolAllocation _allocation;
    CommandBuffer _native;
    Vk _vk;
    DeviceVK _device;

    internal CommandListVK(CommandPoolAllocation allocation, CommandBuffer cmdBuffer) : 
        base(allocation.Pool.Queue)
    {
        _allocation = allocation;
        _native = cmdBuffer;
        _device = allocation.Pool.Queue.VKDevice;
        _vk = allocation.Pool.Queue.VK;
        Semaphore = new SemaphoreVK(_device);
    }

    public override void Free()
    {
        if (IsFree)
            return;

        IsFree = true;
        _allocation.Free(this);
    }

    protected override void OnDispose()
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
}

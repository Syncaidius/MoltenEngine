namespace Molten.Graphics;

public abstract class GpuCommandList : GpuObject
{
    protected GpuCommandList(GpuCommandQueue queue) : 
        base(queue.Device)
    {
        Queue = queue;
    }

    public abstract void Free();

    public GpuCommandQueue Queue { get; }

    public GpuFence Fence { get; set; }

    public uint BranchIndex { get; set; }

    public GpuCommandListFlags Flags { get; set; }

    public GpuCommandList Previous { get; internal set; }
}

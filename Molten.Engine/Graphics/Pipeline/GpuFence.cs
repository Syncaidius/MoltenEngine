namespace Molten.Graphics;

public abstract class GpuFence : GpuObject
{
    protected GpuFence(GpuDevice device) : base(device) { }

    public abstract void Reset();
}

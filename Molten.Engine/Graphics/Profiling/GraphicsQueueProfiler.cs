namespace Molten.Graphics;

/// <summary>
/// Represents profiling statistics for a <see cref="GpuCommandQueue"/>.
/// </summary>
public class GraphicsQueueProfiler
{
    internal void Reset()
    {
        DrawCalls = 0;
        DispatchCalls = 0;

        ResourceMapCalls = 0;
        ResourceCopyCalls = 0;
        SubResourceCopyCalls = 0;
        SubResourceUpdateCalls = 0;

        BindSurfaceCalls = 0;
        BindBufferCalls = 0;
        BindResourceCalls = 0;
    }

    internal void Accumulate(GraphicsQueueProfiler other)
    {
        DrawCalls += other.DrawCalls;
        DispatchCalls += other.DispatchCalls;

        ResourceMapCalls += other.ResourceMapCalls;
        ResourceCopyCalls += other.ResourceCopyCalls;
        SubResourceCopyCalls += other.SubResourceCopyCalls;
        SubResourceUpdateCalls += other.SubResourceUpdateCalls;

        BindSurfaceCalls += other.BindSurfaceCalls;
        BindBufferCalls += other.BindBufferCalls;
        BindResourceCalls += other.BindResourceCalls;
    }

    public uint DrawCalls { get; set; }

    public uint DispatchCalls { get; set; }

    public uint ResourceMapCalls { get; set; }

    public uint ResourceCopyCalls { get; set; }

    public uint SubResourceCopyCalls { get; set; }

    public uint SubResourceUpdateCalls { get; set; }

    public uint BindSurfaceCalls { get; set; }

    public uint BindBufferCalls { get; set; }

    public uint BindResourceCalls { get; set; }

    public uint BindShaderCalls { get; set; }
}

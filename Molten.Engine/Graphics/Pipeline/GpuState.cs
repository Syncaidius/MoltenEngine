
namespace Molten.Graphics;

public class GpuState
{
    internal GpuState(GpuDevice device)
    {
        Device = device;

        uint maxRTs = Device.Capabilities.PixelShader.MaxOutputTargets;
        Viewports = new GpuStateArray<ViewportF>(maxRTs);
        ScissorRects = new GpuStateArray<Rectangle>(maxRTs); // TODO More than one scissor rect can be used during a single draw call to the same output surface.
        Surfaces = new GpuStateValueGroup<IRenderSurface2D>(maxRTs);
        DepthSurface = new GpuStateValue<IDepthStencilSurface>();
        Shader = new GpuStateBasicValue<Shader>();

        VertexBuffers = new GpuStateValueGroup<GpuBuffer>(Device.Capabilities.VertexBuffers.MaxSlots, (vb, slotID) =>
        {
            if (vb.BufferType != GpuBufferType.Vertex)
                throw new GpuResourceException(vb, $"None-vertex buffer ({vb.BufferType}) bound to vertex buffer slot ${slotID}.");
        });

        IndexBuffer = new GpuStateValue<GpuBuffer>((ib) =>
        {
            if (ib.BufferType != GpuBufferType.Index)
                throw new GpuResourceException(ib, $"None-index buffer ({ib.BufferType}) bound to index buffer slot.");
        });
    }

    internal GpuState Clone()
    {
        GpuState clone = new GpuState(Device);
        CopyTo(clone);
        return clone;
    }

    internal void CopyTo(GpuState target)
    {
        Shader.CopyTo(target.Shader);
        Viewports.CopyTo(target.Viewports);
        ScissorRects.CopyTo(target.ScissorRects);
        Surfaces.CopyTo(target.Surfaces);
        DepthSurface.CopyTo(target.DepthSurface);
    }

    public void Reset()
    {
        Surfaces.Reset();
        Viewports.Reset();
        ScissorRects.Reset();
        VertexBuffers.Reset();

        DepthSurface.Value = null;
        Shader.Value = null;
        IndexBuffer.Value = null;
    }

    public GpuDevice Device { get; }

    public GpuStateArray<ViewportF> Viewports { get; }

    public GpuStateArray<Rectangle> ScissorRects { get; }

    public GpuStateValueGroup<IRenderSurface2D> Surfaces { get; }

    public GpuStateValue<IDepthStencilSurface> DepthSurface { get; }

    public GpuStateBasicValue<Shader> Shader { get; }

    public GpuStateValueGroup<GpuBuffer> VertexBuffers { get; }

    public GpuStateValue<GpuBuffer> IndexBuffer { get; }
}

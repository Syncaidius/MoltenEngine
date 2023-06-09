
namespace Molten.Graphics
{
    public class GraphicsState
    {
        internal GraphicsState(GraphicsDevice device)
        {
            Device = device;

            uint maxRTs = Device.Capabilities.PixelShader.MaxOutputTargets;
            Viewports = new GraphicsStateArray<ViewportF>(maxRTs);
            ScissorRects = new GraphicsStateArray<Rectangle>(maxRTs); // TODO More than one scissor rect can be used during a single draw call to the same output surface.
            Surfaces = new GraphicsStateValueGroup<IRenderSurface2D>(maxRTs);
            DepthSurface = new GraphicsStateValue<IDepthStencilSurface>();
            Shader = new GraphicsStateValue<HlslShader>();
            VertexBuffers = new GraphicsStateValueGroup<GraphicsBuffer>(Device.Capabilities.VertexBuffers.MaxSlots);
            IndexBuffer = new GraphicsStateValue<GraphicsBuffer>();
        }

        internal GraphicsState Clone()
        {
            GraphicsState clone = new GraphicsState(Device);
            CopyTo(clone);
            return clone;
        }

        internal void CopyTo(GraphicsState target)
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

        public GraphicsDevice Device { get; }

        public GraphicsStateArray<ViewportF> Viewports { get; }

        public GraphicsStateArray<Rectangle> ScissorRects { get; }

        public GraphicsStateValueGroup<IRenderSurface2D> Surfaces { get; }

        public GraphicsStateValue<IDepthStencilSurface> DepthSurface { get; }

        public GraphicsStateValue<HlslShader> Shader { get; }

        public GraphicsStateValueGroup<GraphicsBuffer> VertexBuffers { get; }

        public GraphicsStateValue<GraphicsBuffer> IndexBuffer { get; }
    }
}

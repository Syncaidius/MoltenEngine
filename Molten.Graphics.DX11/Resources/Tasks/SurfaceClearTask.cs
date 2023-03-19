namespace Molten.Graphics
{
    internal struct SurfaceClearTask : IGraphicsResourceTask
    {
        public RenderSurface2DDX11 Surface;

        public Color Color;

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            Surface.OnClear(cmd as CommandQueueDX11, Color);
            return false;
        }
    }
}

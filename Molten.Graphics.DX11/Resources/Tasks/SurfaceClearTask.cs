namespace Molten.Graphics.DX11
{
    internal struct SurfaceClearTask : IGraphicsResourceTask
    {
        public RenderSurface2DDX11 Surface;

        public Color Color;

        public bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            Surface.Ensure(cmd);
            Surface.OnClear(cmd as GraphicsQueueDX11, Color);
            return false;
        }
    }
}

namespace Molten.Graphics
{
    internal struct SurfaceClearChange : ITextureTask
    {
        public RenderSurface2D Surface;

        public Color Color;

        public bool Process(CommandQueueDX11 cmd, TextureDX11 texture)
        {
            Surface.Clear(cmd, Color);
            return false;
        }
    }
}

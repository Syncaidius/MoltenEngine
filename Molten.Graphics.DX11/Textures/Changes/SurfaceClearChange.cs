namespace Molten.Graphics
{
    internal struct SurfaceClearChange : ITextureTask
    {
        public RenderSurface2D Surface;

        public Color Color;

        public bool Process(CommandQueueDX11 cmd, TextureBase texture)
        {
            Surface.Clear(cmd, Color);
            return false;
        }
    }
}

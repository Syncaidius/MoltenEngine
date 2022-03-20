namespace Molten.Graphics
{
    internal struct SurfaceClearChange : ITextureTask
    {
        public RenderSurface2D Surface;

        public Color Color;

        public bool Process(DeviceContext pipe, TextureBase texture)
        {
            Surface.Clear(pipe, Color);
            return false;
        }
    }
}

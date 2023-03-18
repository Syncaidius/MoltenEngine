namespace Molten.Graphics
{
    internal struct DepthClearChange : ITextureTask
    {
        public DepthSurfaceDX11 Surface;

        public DepthClearFlags Flags;

        public bool Process(CommandQueueDX11 cmd, TextureDX11 texture)
        {
            Surface.Clear(cmd, Flags);
            return false;
        }
    }
}

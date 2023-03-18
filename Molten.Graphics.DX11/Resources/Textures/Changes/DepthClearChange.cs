namespace Molten.Graphics
{
    internal struct DepthClearChange : ITextureTask
    {
        public DepthStencilSurface Surface;

        public DepthClearFlags Flags;

        public bool Process(CommandQueueDX11 cmd, TextureDX11 texture)
        {
            Surface.Clear(cmd, Flags);
            return false;
        }
    }
}

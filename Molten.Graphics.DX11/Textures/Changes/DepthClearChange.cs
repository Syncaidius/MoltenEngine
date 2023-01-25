using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct DepthClearChange : ITextureTask
    {
        public DepthStencilSurface Surface;

        public DepthClearFlags Flags;

        public bool Process(CommandQueueDX11 cmd, TextureBase texture)
        {
            Surface.Clear(cmd, Flags);
            return false;
        }
    }
}

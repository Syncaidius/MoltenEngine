using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal struct TextureResizeTask : IGraphicsResourceTask
    {
        public uint NewWidth;

        public uint NewHeight;

        public uint NewDepth;

        public uint NewMipMapCount;

        public uint NewArraySize;

        public GraphicsFormat NewFormat;

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            TextureDX11 texture = resource as TextureDX11;
            texture.OnSetSize(ref this);
            return true;
        }
    }
}

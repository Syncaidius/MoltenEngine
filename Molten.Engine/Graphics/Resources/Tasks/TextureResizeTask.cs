
namespace Molten.Graphics
{
    public struct TextureResizeTask : IGraphicsResourceTask
    {
        public uint NewWidth;

        public uint NewHeight;

        public uint NewDepth;

        public uint NewMipMapCount;

        public uint NewArraySize;

        public GraphicsFormat NewFormat;

        public bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            GraphicsTexture texture = resource as GraphicsTexture;
            texture.OnSetSize(ref this);
            return true;
        }
    }
}

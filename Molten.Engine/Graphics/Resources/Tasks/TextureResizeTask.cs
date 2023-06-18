
namespace Molten.Graphics
{
    public struct TextureResizeTask : IGraphicsResourceTask
    {
        public TextureDimensions NewDimensions;

        public GraphicsFormat NewFormat;

        public bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            GraphicsTexture texture = resource as GraphicsTexture;
            texture.OnSetSize(ref this);
            return true;
        }
    }
}

namespace Molten.Graphics
{
    public interface IGraphicsResourceTask
    {
        void Process(GraphicsCommandQueue cmd, GraphicsResource resource);
    }
}

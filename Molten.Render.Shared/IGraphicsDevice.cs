namespace Molten.Graphics
{
    public interface IGraphicsDevice
    {
        void MarkForDisposal(PipelineDisposableObject obj);
    }
}

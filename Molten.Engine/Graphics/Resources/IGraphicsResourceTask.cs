namespace Molten.Graphics
{
    public interface IGraphicsResourceTask
    {
        /// <summary>
        /// Invoked when the current <see cref="IGraphicsResourceTask"/> needs to be processed.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="resource"></param>
        /// <returns>True if the <see cref="GraphicsResource"/> was altered.</returns>
        bool Process(GraphicsQueue cmd, GraphicsResource resource);
    }
}

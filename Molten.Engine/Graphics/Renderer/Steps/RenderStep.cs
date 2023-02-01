namespace Molten.Graphics
{
    /// <summary>
    /// A base class for render steps.
    /// </summary>
    internal abstract class RenderStep : IDisposable
    {
        internal virtual void Initialize(RenderService renderer) { }

        internal abstract void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time);

        public abstract void Dispose();
    }
}

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for render steps.
    /// </summary>
    public abstract class RenderStepBase : IDisposable
    {
        public virtual void Initialize(RenderService renderer) { }

        public abstract void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time);

        public abstract void Dispose();
    }
}

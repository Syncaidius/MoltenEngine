namespace Molten.Graphics
{
    /// <summary>
    /// A base class for render steps.
    /// </summary>
    internal abstract class RenderStepBase : IDisposable
    {
        internal abstract void Initialize(RendererDX11 renderer);

        internal abstract void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time);

        public abstract void Dispose();
    }
}

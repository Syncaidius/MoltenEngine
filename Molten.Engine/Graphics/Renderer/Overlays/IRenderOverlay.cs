namespace Molten.Graphics.Overlays
{
    public interface IRenderOverlay
    {
        void OnRender(Timing time, SpriteBatcher sb, SpriteFont font, GraphicsProfiler rendererProfiler, RenderCamera camera);

        /// <summary>
        /// Gets the title of the debug overlay. This must be unique when added to a <see cref="OverlayProvider"/>.
        /// </summary>
        string Title { get; }
    }
}

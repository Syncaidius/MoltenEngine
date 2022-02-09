namespace Molten.Graphics
{
    public interface IRenderSurface : ITexture2D
    {
        /// <summary>Clears the provided <see cref="IRenderSurface"/> with the specified color.</summary>
        /// <param name="surface">The surface.</param>
        /// <param name="color">The color to use for clearing the surface.</param>
        void Clear(Color color);

        /// <summary>Gets the viewport that defines the renderable area of the render target.</summary>
        ViewportF Viewport { get; }
    }
}

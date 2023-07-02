namespace Molten.Graphics
{
    /// <summary>
    /// Represents the implementation of a 1D render surface.
    /// </summary>
    public interface IRenderSurface1D : ITexture1D, IRenderSurface
    {
        /// <summary>Gets the viewport that defines the renderable area of the render target.</summary>
        ViewportF Viewport { get; }
    }
}

namespace Molten.Graphics;

/// <summary>
/// Represents the implementation of a 2D render surface.
/// </summary>
public interface IRenderSurface2D : ITexture2D, IRenderSurface
{
    /// <summary>Gets the viewport that defines the renderable area of the render target.</summary>
    ViewportF Viewport { get; }
}

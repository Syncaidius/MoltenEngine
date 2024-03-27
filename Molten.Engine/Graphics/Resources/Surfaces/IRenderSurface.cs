namespace Molten.Graphics;

/// <summary>
/// Represents the implementation of a render surface.
/// </summary>
public interface IRenderSurface : ITexture
{        
    /// <summary>Clears the provided <see cref="IRenderSurface2D"/> with the specified color.</summary>
    /// <param name="color">The color to use for clearing the surface.</param>
    /// <param name="priority">The priority of the operation.</param>
    void Clear(GpuPriority priority, Color color);

    /// <summary>
    /// Immediately clears the surface to the specified color using the provided command list.
    /// </summary>
    /// <param name="cmd">The command list that will perform the clear operation.</param>
    /// <param name="color">The clear color.</param>
    void ClearImmediate(GpuCommandList cmd, Color color);
}

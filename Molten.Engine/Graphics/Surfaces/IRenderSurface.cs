namespace Molten.Graphics
{
    /// <summary>
    /// Represents the implementation of a render surface.
    /// </summary>
    public interface IRenderSurface
    {        
        /// <summary>Clears the provided <see cref="IRenderSurface2D"/> with the specified color.</summary>
        /// <param name="color">The color to use for clearing the surface.</param>
        /// <param name="priority">The priority of the clear command.</param>
        void Clear(Color color, GraphicsPriority priority);

        /// <summary>
        /// Gets or sets the friendly name of the render surface.
        /// </summary>
        string Name { get; set; }
    }
}

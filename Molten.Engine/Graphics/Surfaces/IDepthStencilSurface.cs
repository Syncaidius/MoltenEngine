namespace Molten.Graphics
{
    public interface IDepthStencilSurface : ITexture2D
    {
        /// <summary>Clears the provided <see cref="IRenderSurface2D"/> with the specified color.</summary>
        /// <param name="surface">The surface.</param>
        /// <param name="flags">The depth-stencil clearing flags.</param>
        /// <param name="depthValue">The value to clear the depth to. Only applies if <see cref="DepthClearFlags.Depth"/> flag was provided.</param>
        /// <param name="stencilValue">The value to clear the stencil to. Only applies if <see cref="DepthClearFlags.Stencil"/> flag was provided.</param>
        void Clear(DepthClearFlags flags, float depthValue = 1.0f, byte stencilValue = 0);

        /// <summary>Resizes the provided <see cref="IDepthStencilSurface"/> to the specified width and height.</summary>
        /// <param name="surface">The surface.</param>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newHeight">The new height.</param>
        void Resize(uint newWidth, uint newHeight);

        /// <summary>Gets the depth-specific format of the surface.</summary>
        DepthFormat DepthFormat { get; }

        /// <summary>Gets the viewport that defines the renderable area of the render target.</summary>
        Viewport Viewport { get; }
    }
}

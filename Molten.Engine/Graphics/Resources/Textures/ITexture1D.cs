namespace Molten.Graphics
{
    /// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
    /// <seealso cref="IDisposable" />
    public interface ITexture1D : ITexture
    {
        /// <summary>
        /// Occurs after the <see cref="ITexture"/> is done resizing. Executed by the renderer thread it is bound to.
        /// </summary>
        event TextureHandler OnResize;

        /// <summary>
        /// Gets a new instance of the texture's <see cref="Texture1DProperties"/> properties.
        /// </summary>
        /// <returns></returns>
        Texture1DProperties Get1DProperties();

        /// <summary>
        /// Resizes a texture to match the specified width, mip-map count and graphics format.
        /// </summary>
        /// <param name="priority">The priority of the copy operation.</param>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newMipMapCount">The new mip-map count.</param>
        /// <param name="format">The new format.</param>
        void Resize(GraphicsPriority priority, uint newWidth, uint newMipMapCount, GraphicsFormat format);

        /// <summary>
        /// Resizes a texture to match the specified width.
        /// </summary>
        /// <param name="priority">The priority of the copy operation.</param>
        /// <param name="newWidth">The new width.</param>
        void Resize(GraphicsPriority priority, uint newWidth);
    }
}

namespace Molten.Graphics
{
    /// <summary>
    /// Represents the implementation of a 3D texture.
    /// </summary>
    public interface ITexture3D : ITexture
    {
        /// <summary>
        /// Gets a new instance of the texture's 3D properties.
        /// </summary>
        /// <returns></returns>
        Texture3DProperties Get3DProperties();

        /// <summary>
        /// Changes the current texture's dimensions and format.
        /// </summary>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newHeight">The new height.</param>
        /// <param name="newMipMapCount">The new mip-map level count.</param>
        /// <param name="newDepth">The new depth of the current <see cref="ITexture3D"/>.</param>
        /// <param name="newFormat">The new graphics format.</param>
        void Resize(uint newWidth, 
            uint newHeight, 
            uint newDepth, 
            uint newMipMapCount = 0, 
            GraphicsFormat newFormat = GraphicsFormat.Unknown);

        /// <summary>Gets the height of the texture.</summary>
        uint Height { get; }

        /// <summary>
        /// The depth (or number of layers) of the 3D texture.
        /// </summary>
        uint Depth { get; }
    }
}

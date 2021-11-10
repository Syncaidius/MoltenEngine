using System;

namespace Molten.Graphics
{
    /// <summary>
    /// A delegate for texture event handlers.
    /// </summary>
    /// <param name="texture">The texture instance that triggered the event.</param>
    public delegate void TextureHandler(ITexture texture);

    /// <summary>Represents a 1D texture, while also acting as the base for all other texture implementations.</summary>
    /// <seealso cref="System.IDisposable" />
    public interface ITexture : IShaderResource
    {
        /// <summary>
        /// Occurs before the <see cref="ITexture"/> has started resizing. Executed by the renderer thread it is bound to.
        /// </summary>
        event TextureHandler OnPreResize;

        /// <summary>
        /// Occurs after the <see cref="ITexture"/> is done resizing. Executed by the renderer thread it is bound to.
        /// </summary>
        event TextureHandler OnPostResize;

        /// <summary>
        /// Gets a new instance of the texture's <see cref="Texture1DProperties"/> properties.
        /// </summary>
        /// <returns></returns>
        Texture1DProperties Get1DProperties();

        /// <summary>
        /// Resizes a texture to match the specified width, mip-map count and graphics format.
        /// </summary>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newMipMapCount">The new mip-map count.</param>
        /// <param name="format">The new format.</param>
        void Resize(int newWidth, int newMipMapCount, GraphicsFormat format);

        /// <summary>
        /// Resizes a texture to match the specified width.
        /// </summary>
        /// <param name="newWidth">The new width.</param>
        void Resize(int newWidth);

        /// <summary>Generates any missing mip-maps for a texture, so long as it's creation flags included <see cref="TextureFlags.AllowMipMapGeneration"/>.</summary>
        void GenerateMipMaps();

        /// <summary>
        /// Copies the current texture to the destination texture. Both textures must be of the same format and dimensions.
        /// </summary>
        /// <param name="destination">The destination texture.</param>
        void CopyTo(ITexture destination);

        /// <summary>
        /// Copies the current texture to the destination texture. Both texture levels must be of the same format and dimensions.
        /// </summary>
        /// <param name="destination">The destination texture.</param>
        /// <param name="destLevel">The destination mip-map level.</param>
        /// <param name="destSlice">The destination array slice.</param>
        /// <param name="sourceLevel">The source mip-map level.</param>
        /// <param name="sourceSlice">The source array slice.</param>
        void CopyTo(int sourceLevel, int sourceSlice, ITexture destination, int destLevel, int destSlice);

        /// <summary>Copies data fom the provided <see cref="TextureData"/> instance into the current texture.</summary>
        /// <param name="data"></param>
        /// <param name="srcMipIndex">The starting mip-map index within the provided <see cref="TextureData"/>.</param>
        /// <param name="srcArraySlice">The starting array slice index within the provided <see cref="TextureData"/>.</param>
        /// <param name="mipCount">The number of mip-map levels to copy per array slice, from the provided <see cref="TextureData"/>.</param>
        /// <param name="arrayCount">The number of array slices to copy from the provided <see cref="TextureData"/>.</param>
        /// <param name="destMipIndex">The mip-map index within the current texture to start copying to.</param>
        /// <param name="destArraySlice">The array slice index within the current texture to start copying to.<</param>
        void SetData(TextureData data, int srcMipIndex, int srcArraySlice, int mipCount, int arrayCount, int destMipIndex = 0, int destArraySlice = 0);

        /// <summary>Copies the provided data into the texture.</summary>
        /// <param name="data">The data to copy to the texture.</param>
        /// <param name="level">The mip-map level to copy the data to.</param>
        /// <param name="count">The number of elements to copy from the provided data array.</param>
        /// <param name="mipIndex">The index at which to start copying from the provided data array.</param>
        /// <param name="arraySlice">The position in the texture array to start copying the texture data to. For a non-array texture, this should be 0.</param>
        void SetData<T>(int level, T[] data, int startIndex, int count, int pitch, int arraySlice = 0) where T : struct;

        /// <summary>Copies the provided data into the texture.</summary>
        /// <param name="data">The slice data to copy to the texture.</param>
        /// <param name="mipCount">The number of mip maps to copy from the source data.</param>
        /// <param name="mipLevel">The mip-map level at which to start copying to within the texture.</param>
        /// <param name="arraySlice">The position in the texture array to start copying the texture data to. For a non-array texture, this should be 0.</param>
        void SetData(TextureData.Slice data, int mipLevel, int arraySlice);

        void SetData<T>(Rectangle area, T[] data, int bytesPerPixel, int level, int arrayIndex = 0) where T : struct;

        /// <summary>Returns the data contained within a texture via a staging texture or directly from the texture itself if possible.</summary>
        /// <param name="stagingTexture">A staging texture to use when retrieving data from the GPU. Only textures
        /// with the staging flag set will work.</param>
        /// <param name="callback">The callback for when the data retrieval is completed.</param>
        void GetData(ITexture stagingTexture, Action<TextureData> callback);

        /// <summary>Returns the data from a single mip-map level within a slice of the texture. For 2D, non-array textures, this will always be slice 0.</summary>
        /// <param name="stagingTexture">The staging texture to copy the data to, from the GPU.</param>
        /// <param name="level">The mip-map level to retrieve.</param>
        /// <param name="arrayIndex">The array slice/index to access.</param>
        /// <param name="callback">The callback for when the data retrieval is completed.</param>
        void GetData(ITexture stagingTexture, int level, int arrayIndex, Action<TextureData.Slice> callback);

        /// <summary>
        /// Returns true if the texture was created with the specified flags.
        /// </summary>
        /// <param name="flags">The flags to check.</param>
        /// <returns>True if the specified flags exist.</returns>
        bool HasFlags(TextureFlags flags);

        /// <summary>Gets the flags that were passed in when the texture was created.</summary>
        TextureFlags Flags { get; }

        /// <summary>Gets the format of the texture.</summary>
        GraphicsFormat Format { get; }

        /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
        bool IsBlockCompressed { get; }

        /// <summary>Gets the width of the texture.</summary>
        int Width { get; }

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        int MipMapCount { get; }

        /// <summary>Gets the number of array slices in the texture.</summary>
        int ArraySize { get; }

        /// <summary>
        /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
        /// </summary>
        int SampleCount { get; }

        /// <summary>
        /// Gets whether or not the texture is multisampled. This is true if <see cref="SampleCount"/> is greater than 1.
        /// </summary>
        bool IsMultisampled { get; }

        /// <summary>
        /// Gets or sets the tag object.
        /// </summary>
        object Tag { get; set; }

        /// <summary>Gets the sort key assigned to the current <see cref="ITexture"/>.</summary>
        int SortKey { get; }

        /// <summary>
        /// Gets the renderer that the texture is bound to.
        /// </summary>
        MoltenRenderer Renderer { get; }
    }
}

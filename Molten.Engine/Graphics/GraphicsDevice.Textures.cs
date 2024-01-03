namespace Molten.Graphics
{
    public abstract partial class GraphicsDevice
    {
        public GraphicsTexture CreateStagingTexture(ITexture src)
        {
            ITexture result = null;

            switch (src.TextureType)
            {
                case GraphicsTextureType.Texture1D:
                    result = CreateTexture1D(src.Width, src.MipMapCount, src.ArraySize, src.ResourceFormat, 
                        GraphicsResourceFlags.AllReadWrite, Name + "_staging");
                    break;

                case GraphicsTextureType.Texture2D:
                    result = CreateTexture2D(src.Width, src.Height, src.MipMapCount, src.ArraySize, src.ResourceFormat, 
                        GraphicsResourceFlags.AllReadWrite, src.MultiSampleLevel, src.SampleQuality, Name + "_staging");
                    break;

                case GraphicsTextureType.Texture3D:
                    result = CreateTexture3D(src.Width, src.Height, src.Depth, src.MipMapCount, src.ResourceFormat, 
                        GraphicsResourceFlags.AllReadWrite, Name + "_staging");
                    break;

                case GraphicsTextureType.TextureCube:
                    ITextureCube cube = this as ITextureCube;
                    result = CreateTextureCube(src.Width, src.Height, src.MipMapCount, src.ResourceFormat, cube.CubeCount, src.ArraySize, 
                        GraphicsResourceFlags.AllReadWrite, Name + "_staging");
                    break;

                default:
                    throw new GraphicsResourceException(src as GraphicsTexture, "Unsupported staging texture type");
            }

            return result as GraphicsTexture;
        }

        /// <summary>
        /// Creates a new 1D texture and returns it.
        /// </summary>
        /// <param name="width">The width of the texture, in pixels.</param>
        /// <param name="mipCount">The number of mip-map levels.</param>
        /// <param name="arraySize">The number of array slices.</param>
        /// <param name="format">The <see cref="GraphicsFormat"/>.</param>
        /// <param name="flags">Resource creation flags.</param>
        /// <param name="name">A custom name for the texture resource, if any.</param>
        /// <returns></returns>
        /// <returns></returns>
        public abstract ITexture1D CreateTexture1D(uint width, uint mipCount, uint arraySize, 
            GraphicsFormat format, GraphicsResourceFlags flags, string name = null);

        /// <summary>Creates a new 1D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        /// <param name="name">A custom name for the texture resource, if any.</param>
        public ITexture1D CreateTexture1D(TextureData data, string name = null)
        {
            ITexture1D tex = CreateTexture1D(data.Width, data.MipMapLevels, data.ArraySize, data.Format, data.Flags, name);
            tex.SetData(GraphicsPriority.Apply, data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        /// <summary>
        /// Creates a new 2D texture and returns it.
        /// </summary>
        /// <param name="width">The width of the texture, in pixels.</param>
        /// <param name="height">The height of the texture, in pixels.</param>
        /// <param name="mipCount">The number of mip-map levels.</param>
        /// <param name="arraySize">The number of array slices.</param>
        /// <param name="format">The <see cref="GraphicsFormat"/>.</param>
        /// <param name="flags">Resource creation flags.</param>
        /// <param name="aaLevel">The number of samples to perform for multi-sampled anti-aliasing (MSAA).</param>
        /// <param name="aaQuality">The quality preset to use for multi-sampled anti-aliasing (MSAA).</param>
        /// <param name="name">A custom name for the texture resource, if any.</param>
        /// <returns></returns>
        public abstract ITexture2D CreateTexture2D(uint width, uint height, uint mipCount, uint arraySize,
                GraphicsFormat format, 
                GraphicsResourceFlags flags,
                AntiAliasLevel aaLevel = AntiAliasLevel.None,
                MSAAQuality aaQuality = MSAAQuality.Default, string name = null);

        /// <summary>Creates a new 2D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        /// <param name="name">A custom name for the texture resource, if any.</param>
        public ITexture2D CreateTexture2D(TextureData data, string name = null)
        {
            ITexture2D tex = CreateTexture2D(data.Width, data.Height, data.MipMapLevels, data.ArraySize, 
                data.Format,
                data.Flags, 
                data.MultiSampleLevel,
                data.MultiSampleQuality,
                name);

            tex.SetData(GraphicsPriority.Apply, data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        /// <summary>
        /// Creates a new 3D texture and returns it.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="depth">The depth of the 3D texture.</param>
        /// <param name="mipCount">The number of mip-map levels.</param>
        /// <param name="format">The <see cref="GraphicsFormat"/>.</param>
        /// <param name="flags">Resource creation flags.</param>
        /// <param name="name">A custom name for the texture resource, if any.</param>
        /// <returns></returns>
        public abstract ITexture3D CreateTexture3D(uint width, uint height, uint depth, uint mipCount,
                       GraphicsFormat format, GraphicsResourceFlags flags, string name = null);

        /// <summary>Creates a new 3D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        /// <param name="name">A custom name for the texture resource, if any.</param>
        public ITexture3D CreateTexture3D(TextureData data, string name = null)
        {
            ITexture3D tex = CreateTexture3D(data.Width, data.Height, data.Depth, data.MipMapLevels, data.Format, data.Flags, name);
            tex.SetData(GraphicsPriority.Apply, data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        /// <summary>
        /// Creates a new cube texture (cube-map) and returns it.
        /// </summary>
        /// <param name="width">The width of the texture, in pixels.</param>
        /// <param name="height">The height of the texture, in pixels.</param>
        /// <param name="mipCount">The number of mip-map levels.</param>
        /// <param name="cubeCount">The number texture cubes (cube array slices).</param>
        /// <param name="format">The <see cref="GraphicsFormat"/>.</param>
        /// <param name="flags">Resource creation flags.</param>
        /// <param name="arraySize">The number of array slices.</param>
        /// <param name="name">A custom name for the texture resource, if any.</param>
        /// <returns></returns>
        public abstract ITextureCube CreateTextureCube(uint width, uint height, uint mipCount, GraphicsFormat format,
            uint cubeCount = 1, uint arraySize = 1, GraphicsResourceFlags flags = GraphicsResourceFlags.None, string name = null);

        /// <summary>Creates a new cube texture (cube-map) and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        /// <param name="cubeCount">The number of cubes to be represented in the texture. The <see cref="ITexture.ArraySize"/> will be <paramref name="cubeCount"/> * 6.</param>
        /// <param name="name">A custom name for the texture resource, if any.</param>
        public ITextureCube CreateTextureCube(TextureData data, uint cubeCount = 1, string name = null)
        {
            ITextureCube tex = CreateTextureCube(data.Width, data.Height, data.MipMapLevels, data.Format, cubeCount, data.ArraySize, data.Flags, name);
            tex.SetData(GraphicsPriority.Apply, data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        /// <summary>
        /// Resolves a source texture into a destination texture. <para/>
        /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
        /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        public abstract void ResolveTexture(GraphicsTexture source, GraphicsTexture destination);

        /// <summary>Resources the specified sub-resource of a source texture into the sub-resource of a destination texture.</summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        /// <param name="sourceMipLevel">The source mip-map level.</param>
        /// <param name="sourceArraySlice">The source array slice.</param>
        /// <param name="destMiplevel">The destination mip-map level.</param>
        /// <param name="destArraySlice">The destination array slice.</param>
        public abstract void ResolveTexture(GraphicsTexture source, GraphicsTexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice);
    }
}

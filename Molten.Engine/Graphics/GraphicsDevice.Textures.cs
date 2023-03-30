namespace Molten.Graphics
{
    public abstract partial class GraphicsDevice
    {
        /// <summary>Creates a new 1D texture and returns it.</summary>
        /// <param name="properties">A set of 1D texture properties.</param>
        public abstract ITexture CreateTexture1D(Texture1DProperties properties, bool allowMipMapGen = false, string name = null);

        /// <summary>Creates a new 1D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        public abstract ITexture CreateTexture1D(TextureData data, bool allowMipMapGen = false, string name = null);

        /// <summary>Creates a new 2D texture and returns it.</summary>
        /// <param name="properties">A set of 2D texture properties.</param>
        public abstract ITexture2D CreateTexture2D(Texture2DProperties properties, bool allowMipMapGen = false, string name = null);

        /// <summary>Creates a new 2D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        public abstract ITexture2D CreateTexture2D(TextureData data, bool allowMipMapGen = false, string name = null);

        /// <summary>Creates a new 3D texture and returns it.</summary>
        /// <param name="properties">A set of 3D texture properties.</param>
        public abstract ITexture3D CreateTexture3D(Texture3DProperties properties, bool allowMipMapGen = false, string name = null);

        /// <summary>Creates a new 3D texture and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        public abstract ITexture3D CreateTexture3D(TextureData data, bool allowMipMapGen = false, string name = null);

        /// <summary>Creates a new cube texture (cube-map) and returns it.</summary>
        /// <param name="properties">A set of 2D texture properties.</param>
        public abstract ITextureCube CreateTextureCube(Texture2DProperties properties, bool allowMipMapGen = false, string name = null);

        /// <summary>Creates a new cube texture (cube-map) and returns it.</summary>
        /// <param name="data">The data from which to create the texture.</param>
        public abstract ITextureCube CreateTextureCube(TextureData data, bool allowMipMapGen = false, string name = null);

        /// <summary>
        /// Resolves a source texture into a destination texture. <para/>
        /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
        /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        public abstract void ResolveTexture(ITexture source, ITexture destination);

        /// <summary>Resources the specified sub-resource of a source texture into the sub-resource of a destination texture.</summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        /// <param name="sourceMipLevel">The source mip-map level.</param>
        /// <param name="sourceArraySlice">The source array slice.</param>
        /// <param name="destMiplevel">The destination mip-map level.</param>
        /// <param name="destArraySlice">The destination array slice.</param>
        public abstract void ResolveTexture(ITexture source, ITexture destination, uint sourceMipLevel, uint sourceArraySlice, uint destMiplevel, uint destArraySlice);
    }
}

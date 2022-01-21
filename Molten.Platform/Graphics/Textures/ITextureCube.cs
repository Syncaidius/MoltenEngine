namespace Molten.Graphics
{
    public interface ITextureCube : ITexture
    {
        void Resize(uint newWidth, uint newHeight, uint newMipMapCount);

        /// <summary>Gets the height of the texture.</summary>
        uint Height { get; }

        /// <summary>Gets the number of cube maps stored in the texture. This is greater than 1 if the texture is a cube-map array.</summary>
        uint CubeCount { get; }
    }
}

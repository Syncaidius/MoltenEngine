namespace Molten.Graphics
{
    public interface ITextureCube : ITexture
    {
        void Resize(int newWidth, int newHeight, int newMipMapCount);

        /// <summary>Gets the height of the texture.</summary>
        int Height { get; }

        /// <summary>Gets the number of cube maps stored in the texture. This is greater than 1 if the texture is a cube-map array.</summary>
        int CubeCount { get; }
    }
}

namespace Molten.Graphics;

public interface ITextureCube : ITexture2D
{
    /// <summary>Gets the number of cube maps stored in the texture. This is greater than 1 if the texture is a cube-map array.</summary>
    uint CubeCount { get; }
}

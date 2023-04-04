namespace Molten
{
    /// <summary>
    /// Represents various modes of culling for the rasterizer.
    /// </summary>
    public enum RasterizerCullingMode
    {
        /// <summary>
        /// No faces are culled during rasterization.
        /// </summary>
        None = 0x1,

        /// <summary>
        /// Front-facing faces are culled during rasterization.
        /// </summary>
        Front = 0x2,

        /// <summary>
        /// Back-facing faces are culled during rasterization.
        /// </summary>
        Back = 0x3,

        /// <summary>
        /// All faces are culled during rasterization.
        /// </summary>
        All = 0x4,
    }
}

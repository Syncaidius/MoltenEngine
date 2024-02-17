using Silk.NET.Core;

namespace Molten.Graphics;

[Flags]
public enum GraphicsCapabilityFlags : ulong
{
    None = 0,

    /// <summary>
    /// Blend-state logic operations are supported.
    /// </summary>
    BlendLogicOp = 1,

    /// <summary>
    /// Non-power-of-two textures are supported.
    /// </summary>
    NonPowerOfTwoTextures = 1 << 1,

    /// <summary>
    /// Depth bounds testing is supported.
    /// </summary>
    DepthBoundsTesting = 1 << 2,

    /// <summary>
    /// Occulsion queries are supported.
    /// </summary>
    OcculsionQueries = 1 << 3,

    /// <summary>
    /// Hardware instancing is supported.
    /// </summary>
    HardwareInstancing = 1 << 4,

    /// <summary>
    /// Concurrent resource creation is supported.
    /// <para>True means resources can be created concurrently on multiple threads while drawing.</para>
    /// </summary>
    ConcurrentResourceCreation = 1 << 5,

    /// <summary>
    /// Texture cube arrays are supported.
    /// </summary>
    TextureCubeArrays= 1 << 6,

    /// <summary>
    /// Rasterizer-order-views (ROVs) support/requirement
    /// </summary>
    RasterizerOrderViews = 1 << 7,

    /// <summary>
    /// Tile-based rendering is supported.
    /// </summary>
    TileBasedRendering = 1 << 8,
}

public static class GraphicsCapabilityFlagsExtensions
{
    public static bool Has(this GraphicsCapabilityFlags flags, GraphicsCapabilityFlags flag)
    {
        return (flags & flag) == flag;
    }

    public static GraphicsCapabilityFlags ToCapFlag(this Bool32 value, GraphicsCapabilityFlags flag)
    {
        return value ? flag : GraphicsCapabilityFlags.None;
    }

    public static GraphicsCapabilityFlags ToCapFlag(this bool value, GraphicsCapabilityFlags flag)
    {
        return value ? flag : GraphicsCapabilityFlags.None;
    }
}

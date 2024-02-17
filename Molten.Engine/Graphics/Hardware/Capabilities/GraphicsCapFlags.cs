using Silk.NET.Core;

namespace Molten.Graphics;

/// <summary>
/// Flags which represent the capabilities of a <see cref="GraphicsDevice"/>.
/// </summary>
[Flags]
public enum GraphicsCapFlags : ulong
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

    /// <summary>
    /// Wave operations are supported in HLSL/SPIR-V shaders.
    /// <para>See: https://gpuopen.com/wp-content/uploads/2017/07/GDC2017-Wave-Programming-D3D12-Vulkan.pdf</para>
    /// </summary>
    WaveOperations = 1 << 9,

    /// <summary>
    /// Alpha blending factors are supported.
    /// </summary>
    AlphaBlendFactor = 1 << 10,

    /// <summary>
    /// Barycentric intrinsic instructions are supported in the rasterizer stage.
    /// <para>See: https://gpuopen.com/learn/barycentrics12-dx12-gcnshader-ext-sample/</para>
    /// </summary>
    Barycentrics = 1 << 11,
}

public static class GraphicsCapabilityFlagsExtensions
{
    public static bool Has(this GraphicsCapFlags flags, GraphicsCapFlags flag)
    {
        return (flags & flag) == flag;
    }

    public static GraphicsCapFlags ToCapFlag(this Bool32 value, GraphicsCapFlags flag)
    {
        return value ? flag : GraphicsCapFlags.None;
    }

    public static GraphicsCapFlags ToCapFlag(this bool value, GraphicsCapFlags flag)
    {
        return value ? flag : GraphicsCapFlags.None;
    }
}

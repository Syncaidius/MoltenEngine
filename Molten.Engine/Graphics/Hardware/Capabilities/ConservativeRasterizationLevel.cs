namespace Molten.Graphics;

/// <summary>
/// Equivilent to D3D12_CONSERVATIVE_RASTERIZATION_TIER values in the DX12 API.
/// </summary>
public enum ConservativeRasterizationLevel
{
    /// <summary>
    /// Conservative rasterization is not supported.
    /// </summary>
    NotSupported = 0,

    /// <summary>
    /// Tier 1 enforces a maximum 1/2 pixel uncertainty region and does not support post-snap degenerates. 
    /// <para>This tier is good for tiled rendering, a texture atlas, light map generation and sub-pixel shadow maps.</para>
    /// </summary>
    Level1 = 1,

    /// <summary>
    /// Tier 2 reduces the maximum uncertainty region to 1/256 and requires post-snap degenerates not be culled. 
    /// <para>This tier is helpful for CPU-based algorithm acceleration (such as voxelization).</para>
    /// </summary>
    Level2 = 2,

    /// <summary>
    /// Tier 3 maintains a maximum 1/256 uncertainty region and adds support for inner input coverage. 
    /// Inner input coverage adds the new value SV_InnerCoverage to High Level Shading Language (HLSL). 
    /// <para>This is a 32-bit scalar integer that can be specified on input to a pixel shader, 
    /// and represents the underestimated conservative rasterization information (that is, whether a pixel is guaranteed-to-be-fully covered). 
    /// </para>
    /// <para>This tier is helpful for occlusion culling.</para>
    /// </summary>
    Level3 = 3,
}

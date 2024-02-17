namespace Molten.Graphics;

/// <summary>
/// Equivilent to D3D12_CONSERVATIVE_RASTERIZATION_TIER values in the DX12 API.
/// </summary>
[Flags]
public enum ConservativeRasterizationFlags : byte
{
    /// <summary>
    /// Conservative rasterization is not supported.
    /// </summary>
    NotSupported = 0,

    /// <summary>
    /// Enforces a maximum 1/2 pixel uncertainty region and does not support post-snap degenerates. 
    /// <para>This is good for tiled rendering, a texture atlas, light map generation and sub-pixel shadow maps.</para>
    /// </summary>
    Uncertainty1_2 = 1,

    /// <summary>
    /// Tier 2 reduces the maximum uncertainty region to 1/256. 
    /// <para>This is helpful for CPU-based algorithm acceleration (such as voxelization).</para>
    /// </summary>
    Uncertainty1_256 = 1 << 1,

    /// <summary>
    /// Post-snap degenerates are supported if not culled.
    /// </summary>
    UnculledPostSnapDegenerates = 1 << 2,

    /// <summary>
    /// Inner input coverage adds the new value SV_InnerCoverage to High Level Shading Language (HLSL). 
    /// <para>This is a 32-bit scalar integer that can be specified on input to a pixel shader, 
    /// and represents the underestimated conservative rasterization information (that is, whether a pixel is guaranteed-to-be-fully covered). 
    /// </para>
    /// <para>This tier is helpful for occlusion culling.</para>
    /// </summary>
    InnerInputCoverageSupport = 3,
}

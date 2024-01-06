namespace Molten.Graphics;

public enum GraphicsApi
{
    /// <summary>
    /// The graphics API is either unsupported or not-known.
    /// </summary>
    Unsupported = 0,

    /// <summary>
    /// DirectX 11 or 12 with feature level 10_0.
    /// </summary>
    DirectX10_0 = 8,

    /// <summary>
    /// DirectX 11 or 12 with feature level 10_1.
    /// </summary>
    DirectX10_1 = 9,

    /// <summary>
    /// DirectX 11.0.
    /// <para>If using DirectX 12, this will be equvilent to feature-level 11_0.</para>
    /// </summary>
    DirectX11_0 = 10,

    /// <summary>
    /// DirectX 11.1 Runtime. Covers DirectX 11.0 to 11.4 feature-sets. 
    /// <para>If using DirectX 12, this will be equvilent to feature-level 11_1.</para>
    /// </summary>
    DirectX11_1 = 11,

    /// <summary>
    /// DirectX 12.0.
    /// </summary>
    DirectX12_0 = 12,

    /// <summary>
    /// DirectX 12.1.
    /// </summary>
    DirectX12_1 = 13,

    /// <summary>
    /// DirectX 12.2.
    /// </summary>
    DirectX12_2 = 14,

    /// <summary>
    /// Vulkan 1.0.
    /// </summary>
    Vulkan1_0 = 32,

    /// <summary>
    /// Vulkan 1.1.
    /// </summary>
    Vulkan1_1 = 33,

    /// <summary>
    /// Vulkan 1.2.
    /// </summary>
    Vulkan1_2 = 34,

    /// <summary>
    /// Vulkan 1.3.
    /// </summary>
    Vulkan1_3 = 35,
}

namespace Molten.Graphics;

public enum DeviceVendor
{
    /// <summary>
    /// The vendor could not be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Advanced Micro Devices.
    /// </summary>
    AMD = 1,

    /// <summary>
    /// Intel Corporation.
    /// </summary>
    Intel = 2,

    /// <summary>
    /// Nvidia Corporation.
    /// </summary>
    Nvidia = 3,

    // ====== Vulkan vendor IDs =======
    // See: https://registry.khronos.org/vulkan/specs/1.3-extensions/html/vkspec.html#VkVendorId

    VIV = 0x10001,

    VSI = 0x10002,

    KAZAN = 0x10003,

    CODEPLAY = 0x10004,

    MESA = 0x10005,

    POCL = 0x10006,
}

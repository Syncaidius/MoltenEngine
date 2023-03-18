namespace Molten.Graphics
{
    /// <summary>
    /// See for info: https://docs.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_standard_multisample_quality_levels
    /// </summary>
    public enum MSAASupport : uint
    {
        /// <summary>
        /// Multi-sampling not supported by hardware.
        /// </summary>
        NotSupported = 0,

        /// <summary>
        /// The hardware only supports standard-pattern or center-pattern multi-sampling. No extra vendor-specific ones are supported.
        /// </summary>
        FixedOnly = 1,
    }
}

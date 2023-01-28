namespace Molten.Graphics
{
    /// <summary>
    /// See for info: https://docs.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_standard_multisample_quality_levels
    /// </summary>
    public enum MSAAQuality : uint
    {
        Default = 0,

        /// <summary>
        /// See: D3D11_STANDARD_MULTISAMPLE_PATTERN 
        /// </summary>
        StandardPattern = 0xffffffff,

        /// <summary>
        /// See: D3D11_CENTER_MULTISAMPLE_PATTERN
        /// </summary>
        CenterPattern = 0xfffffffe
    }
}

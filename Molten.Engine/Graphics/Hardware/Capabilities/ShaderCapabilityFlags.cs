namespace Molten.Graphics;

[Flags]
public enum ShaderCapabilityFlags : ulong
{
    /// <summary>
    /// Indicates that the shader stage is not supported by the current hardware.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates whether the shader capability is supported.
    /// </summary>
    IsSupported = 1,

    /// <summary>
    /// Indicates support for 10-bit-precision floating point values.
    /// </summary>
    Float10 = 1 << 1,

    /// <summary>
    /// Indicates support for 16-bit integer values.
    /// </summary>
    Int16 = 1 << 2,

    /// <summary>
    /// Indicates support for 16-bit, half-precision floating point values.
    /// </summary>
    Float16 = 1 << 3,

    /// <summary>
    /// Indicates support for 64-bit integer values.
    /// </summary>
    Int64 = 1 << 4,

    /// <summary>
    /// Indicates support for double-precision floating point values.
    /// </summary>
    Float64 = 1 << 5,
}

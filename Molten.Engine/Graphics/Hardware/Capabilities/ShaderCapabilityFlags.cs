using Silk.NET.Core;

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

public static class ShaderCapabilityFlagsExtensions
{
    public static bool Has(this ShaderCapabilityFlags flags, ShaderCapabilityFlags flag)
    {
        return (flags & flag) == flag;
    }

    public static ShaderCapabilityFlags ToCapFlag(this Bool32 value, ShaderCapabilityFlags flag)
    {
        return value ? flag : ShaderCapabilityFlags.None;
    }

    public static ShaderCapabilityFlags ToCapFlag(this bool value, ShaderCapabilityFlags flag)
    {
        return value ? flag : ShaderCapabilityFlags.None;
    }
}
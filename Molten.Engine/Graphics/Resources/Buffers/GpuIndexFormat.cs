namespace Molten.Graphics;

/// <summary>Represents the index format. This can either be a 32-bit or 16-bit unsigned value.
/// <para>The value each <see cref="GpuIndexFormat"/> represent the stride of the index type, in bytes.</para></summary>
public enum GpuIndexFormat
{
    /// <summary>
    /// No index buffer format.
    /// </summary>
    None = 0,

    /// <summary>A unsigned 32-bit integer (uint).</summary>
    UInt32 = 4,

    /// <summary>A unsigned 16-bit integer (short).</summary>
    UInt16 = 2,
}

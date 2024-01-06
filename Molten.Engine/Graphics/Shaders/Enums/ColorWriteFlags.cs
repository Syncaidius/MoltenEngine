namespace Molten.Graphics;

/// <summary>
/// Specifies which color channels are enabled for writing.
/// </summary>
[Flags]
public enum ColorWriteFlags : byte
{
    /// <summary>
    /// No color channels are enabled for writing.
    /// </summary>
    None = 0,

    /// <summary>
    /// The red color channel is enabled for writing.
    /// </summary>
    Red = 0x1,

    /// <summary>
    /// The green color channel is enabled for writing.
    /// </summary>
    Green = 0x2,

    /// <summary>
    /// The blue color channel is enabled for writing.
    /// </summary>
    Blue = 0x4,

    /// <summary>
    /// The alpha color channel is enabled for writing.
    /// </summary>
    Alpha = 0x8,

    /// <summary>
    /// All color channels are enabled for writing.
    /// </summary>
    All = 0xF
}

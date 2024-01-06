namespace Molten.Graphics;

/// <summary>
/// Flags indicating how an image is stretched to fit a given monitor's resolution.
/// </summary>
[Flags]
public enum DisplayScalingMode
{
    /// <summary>
    /// Unspecified scaling.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Specifies no scaling. The image is centered on the display. This flag is typically used for a fixed-dot-pitch display (such as an LED display).
    /// </summary>
    Centered = 1,

    /// <summary>
    /// Specifies stretched scaling.
    /// </summary>
    Stretched = 2
}


namespace Molten.Graphics;

/// <summary>
/// Represents a display mode implementation.
/// </summary>
public interface IDisplayMode
{
    /// <summary>
    /// Gets the width of the current <see cref="IDisplayMode"/>, in pixels.
    /// </summary>
    uint Width { get; }

    /// <summary>
    /// Gets the height of the current <see cref="IDisplayMode"/>, in pixels.
    /// </summary>
    uint Height { get; }

    /// <summary>
    /// Gets the refresh rate of the current <see cref="IDisplayMode"/>, in hertz.
    /// </summary>
    uint RefreshRate { get; }

    /// <summary>
    /// Gets the format of the current <see cref="IDisplayMode"/>.
    /// </summary>
    GraphicsFormat Format { get; }

    /// <summary>
    /// Gets the scaling mode of the current <see cref="IDisplayMode"/>.
    /// </summary>
    DisplayScalingMode Scaling { get; }

    /// <summary>
    /// Gets whether the current <see cref="IDisplayMode"/> is a stereo-present mode, capable of presenting a stereo swap-chain, generally used by VR headsets.
    /// </summary>
    bool StereoPresent { get; }
}

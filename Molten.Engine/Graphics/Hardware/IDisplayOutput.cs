namespace Molten.Graphics;

/// <summary>An interface for a display device, usually attached to a <see cref="GpuDevice"/> device.</summary>
public interface IDisplayOutput
{
    /// <summary>
    /// Gets a list of all the supported display modes for a given <see cref="GpuResourceFormat"/>.
    /// </summary>
    /// <param name="format">The format to check for support.</param>
    /// <returns></returns>
    IReadOnlyList<IDisplayMode> GetModes(GpuResourceFormat format);

    /// <summary>Gets the name of the current <see cref="IDisplayOutput"/>.</summary>
    string Name { get; }

    /// <summary>Gets the orientation of the current <see cref="IDisplayOutput"/>.</summary>
    DisplayOrientation Orientation { get; }

    /// <summary>Gets the <see cref="GpuDevice"/> the <see cref="IDisplayOutput"/> is attached to.</summary>
    GpuDevice Device { get; }

    /// <summary>
    /// Gets the desktop bounds of the display.
    /// </summary>
    Rectangle DesktopBounds { get; }
}

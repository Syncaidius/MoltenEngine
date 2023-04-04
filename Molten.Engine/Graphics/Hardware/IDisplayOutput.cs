namespace Molten.Graphics
{
    /// <summary>An interface for a display device, usually attached to a <see cref="IDisplayAdapter"/> device.</summary>
    public interface IDisplayOutput
    {
        /// <summary>
        /// Gets a list of all the supported display modes for a given <see cref="GraphicsFormat"/>.
        /// </summary>
        /// <param name="format">The format to check for support.</param>
        /// <returns></returns>
        IReadOnlyList<IDisplayMode> GetModes(GraphicsFormat format);

        /// <summary>Gets the name of the current <see cref="IDisplayOutput"/>.</summary>
        string Name { get; }

        /// <summary>Gets the orientation of the current <see cref="IDisplayOutput"/>.</summary>
        DisplayOrientation Orientation { get; }

        /// <summary>Gets the <see cref="GraphicsDevice"/> the <see cref="IDisplayOutput"/> is attached to.</summary>
        GraphicsDevice Device { get; }

        /// <summary>
        /// Gets the desktop bounds of the display.
        /// </summary>
        Rectangle DesktopBounds { get; }
    }
}

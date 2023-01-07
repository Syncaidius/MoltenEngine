namespace Molten.Graphics
{
    public delegate void DisplayManagerEvent(IDisplayManager manager);

    /// <summary>
    /// Represents an implementation of a graphics device, adapter and/or display manager.
    /// </summary>
    public interface IDisplayManager : IDisposable
    {
        /// <summary>Initializes the display manager using the provided <see cref="GraphicsSettings"/>.</summary>
        /// <param name="logger">The logger that the current <see cref="IDisplayManager"/> should output information to.</param>
        /// <param name="settings">An instance of <see cref="GraphicsSettings"/>. This is used to help configure the display manager.</param>
        void Initialize(Logger logger, GraphicsSettings settings);

        /// <summary>
        /// Gets a list of all detected <see cref="IDisplayAdapter"/> on the current machine.
        /// </summary>
        IReadOnlyList<IDisplayAdapter> Adapters { get; }

        /// <summary>
        /// Adds all adapters that are compatible with the provided <paramref name="capabilities"/>, to the provided output list.
        /// </summary>
        /// <param name="capabilities">The capabilities for which to test.</param>
        /// <param name="adapters">The list in which to add compatibel <see cref="IDisplayAdapter"/> instances.</param>
        void GetCompatibleAdapters(GraphicsCapabilities capabilities, List<IDisplayAdapter> adapters);

        /// <summary>Gets the system's default display adapter.</summary>
        IDisplayAdapter DefaultAdapter { get; }

        /// <summary>Gets or sets the adapter currently selected for use by the engine.</summary>
        IDisplayAdapter SelectedAdapter { get; set; }

        /// <summary>
        /// Attempts to find a <see cref="IDisplayAdapter"/> with an ID matching the one provided.
        /// </summary>
        /// <param name="id">The <see cref="DeviceID"/> to match.</param>
        /// <returns>A matching <see cref="IDisplayAdapter"/>, or null if no match was found.</returns>
        IDisplayAdapter this[DeviceID id] { get; }
    }
}

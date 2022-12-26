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

        /// <summary>Detects  </summary>
        /// <param name="output">The list in which to add the results.</param>
        void GetAdapters(List<IDisplayAdapter> output);

        /// <summary>Adds all adapters with at least one output attached, to the provided output list.</summary>
        /// <param name="output">The list in which to add the results.</param>
        void GetAdaptersWithOutputs(List<IDisplayAdapter> output);

        /// <summary>Gets the adapter at the specified listing ID. This may change if the physical hardware is altered or swapped around.</summary>
        /// <param name="id">The ID.</param>
        IDisplayAdapter GetAdapter(int id);

        /// <summary>Gets the number of display adapters attached to the system.</summary>
        int AdapterCount { get; }

        /// <summary>Gets the system's default display adapter.</summary>
        IDisplayAdapter DefaultAdapter { get; }

        /// <summary>Gets or sets the adapter currently selected for use by the engine.</summary>
        IDisplayAdapter SelectedAdapter { get; set; }
    }
}

namespace Molten.Graphics
{
    /// <summary>
    /// Represents the implementation of a display or graphics adapter, commonly known as a GPU.
    /// </summary>
    public interface IDisplayAdapter
    {
        /// <summary>Occurs when a connected <see cref="IDisplayOutput"/> is activated on the current <see cref="IDisplayAdapter"/>.</summary>
        event DisplayOutputChanged OnOutputActivated;

        /// <summary>Occurs when a connected <see cref="IDisplayOutput"/> is deactivated on the current <see cref="IDisplayAdapter"/>.</summary>
        event DisplayOutputChanged OnOutputDeactivated;

        /// <summary>
        /// Activates a <see cref="IDisplayOutput"/> on the current <see cref="IDisplayAdapter"/>.
        /// </summary>
        /// <param name="output">The output to be activated.</param>
        void AddActiveOutput(IDisplayOutput output);

        /// <summary>
        /// Deactivates a <see cref="IDisplayOutput"/> from the current <see cref="IDisplayAdapter"/>. It will still be listed in <see cref="Outputs"/>, if attached.
        /// </summary>
        /// <param name="output">The output to be deactivated.</param>
        void RemoveActiveOutput(IDisplayOutput output);

        /// <summary>
        /// Removes all active <see cref="IDisplayOutput"/> from the current <see cref="IDisplayAdapter"/>. They will still be listed in <see cref="Outputs"/>, if attached.
        /// </summary>
        void RemoveAllActiveOutputs();

        /// <summary>Gets the name of the adapter.</summary>
        string Name { get; }

        /// <summary>Gets the machine-local device ID of the current <see cref="IDisplayAdapter"/>.</summary>
        DeviceID ID { get; }

        /// <summary>The hardware vendor.</summary>
        DeviceVendor Vendor { get; }

        /// <summary>
        /// Gets the <see cref="DisplayAdapterType"/> of the current <see cref="IDisplayAdapter"/>.
        /// </summary>
        DisplayAdapterType Type { get; }

        /// <summary>Gets the <see cref="DisplayManager"/> that spawned the adapter.</summary>
        DisplayManager Manager { get; }

        /// <summary>Gets a list of all <see cref="IDisplayOutput"/> devices attached to the current <see cref="IDisplayAdapter"/>.</summary>
        IReadOnlyList<IDisplayOutput> Outputs { get; }

        /// <summary>Gets a list of all active <see cref="IDisplayOutput"/> devices attached to the current <see cref="IDisplayAdapter"/>.
        /// <para>Active outputs are added via <see cref="AddActiveOutput(IDisplayOutput)"/>.</para></summary>
        IReadOnlyList<IDisplayOutput> ActiveOutputs { get; }

        /// <summary>
        /// Gets the capabilities of the current <see cref="IDisplayAdapter"/>.
        /// </summary>
        GraphicsCapabilities Capabilities { get; }
    }
}

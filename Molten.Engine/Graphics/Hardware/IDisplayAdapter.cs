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

        /// <summary>Gets all <see cref="IDisplayOutput"/> devices attached to the current <see cref="IDisplayAdapter"/>.</summary>
        /// <param name="outputList">The output list.</param>
        void GetAttachedOutputs(List<IDisplayOutput> outputList);

        void GetActiveOutputs(List<IDisplayOutput> outputList);

        IDisplayOutput GetOutput(int id);

        void AddActiveOutput(IDisplayOutput output);

        void RemoveActiveOutput(IDisplayOutput output);

        void RemoveAllActiveOutputs();

        /// <summary>Gets the name of the adapter.</summary>
        string Name { get; }

        /// <summary>Gets the amount of dedicated video memory, in megabytes.</summary>
        double DedicatedVideoMemory { get; }

        /// <summary>Gets the amount of system memory dedicated to the adapter, in megabytes.</summary>
        double DedicatedSystemMemory { get; }

        /// <summary>Gets the amount of system memory that is being shared with the adapter, in megabytes.</summary>
        double SharedSystemMemory { get; }

        /// <summary>Gets the machine-local device ID of the current <see cref="IDisplayAdapter"/>.</summary>
        DeviceID ID { get; }

        /// <summary>The hardware vendor.</summary>
        DeviceVendor Vendor { get; }

        /// <summary>
        /// Gets the <see cref="DisplayAdapterType"/> of the current <see cref="IDisplayAdapter"/>.
        /// </summary>
        DisplayAdapterType Type { get; }

        /// <summary>Gets the number of <see cref="IDisplayOutput"/> connected to the current <see cref="IDisplayAdapter"/>.</summary>
        int OutputCount { get; }

        /// <summary>Gets the <see cref="IDisplayManager"/> that spawned the adapter.</summary>
        IDisplayManager Manager { get; }

        /// <summary>
        /// Gets the capabilities of the current <see cref="IDisplayAdapter"/>.
        /// </summary>
        GraphicsCapabilities Capabilities { get; }
    }
}

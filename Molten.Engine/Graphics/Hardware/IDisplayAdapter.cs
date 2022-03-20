namespace Molten.Graphics
{
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

        /// <summary>Gets the listing ID of the <see cref="IDisplayAdapter"/>.</summary>
        int ID { get; }

        /// <summary>The hardware vendor.</summary>
        GraphicsAdapterVendor Vendor { get; }

        /// <summary>Gets the number of <see cref="IDisplayOutput"/> connected to the current <see cref="IDisplayAdapter"/>.</summary>
        int OutputCount { get; }

        /// <summary>Gets the <see cref="IDisplayManager"/> that spawned the adapter.</summary>
        IDisplayManager Manager { get; }
    }
}

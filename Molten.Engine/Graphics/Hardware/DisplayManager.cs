namespace Molten.Graphics
{
    public delegate void DisplayManagerEvent(DisplayManager manager);

    /// <summary>
    /// Represents an implementation of a graphics device, adapter and/or display manager.
    /// </summary>
    public abstract class DisplayManager : EngineObject
    {
        /// <summary>Initializes the display manager using the provided <see cref="GraphicsSettings"/>.</summary>
        /// <param name="settings">An instance of <see cref="GraphicsSettings"/>. This is used to help configure the display manager.</param>
        public void Initialize(Logger log, GraphicsSettings settings)
        {
            OnInitialize(log, settings);

            // Output detection info into renderer log.
            log.WriteLine($"Detected {Adapters.Count} adapters:");
            int aID = 1;

            foreach (IDisplayAdapter adapter in Adapters)
                LogAdapter(log, adapter, aID++);

            IDisplayAdapter preferredAdapter = ValidateAdapterSettings(log, settings);

            // Add all preferred displays to active list
            foreach (int id in settings.DisplayOutputIds.Values)
            {
                if (id < preferredAdapter.Outputs.Count)
                    preferredAdapter.AddActiveOutput(preferredAdapter.Outputs[id]);
            }

            // Log preferred adapter stats
            log.WriteLine($"Chosen {preferredAdapter.Name}");
        }

        private IDisplayAdapter ValidateAdapterSettings(Logger log, GraphicsSettings gfxSettings)
        {
            // Does the chosen device ID from settings still match any of our detected devices?
            DeviceID selectedID = gfxSettings.AdapterID;
            IDisplayAdapter preferredAdapter = this[selectedID];
            if (preferredAdapter == null)
            {
                preferredAdapter = DefaultAdapter;
                gfxSettings.AdapterID.Value = preferredAdapter.ID;
                gfxSettings.DisplayOutputIds.Values.Clear();

                if (selectedID.ID != 0)
                {
                    if (preferredAdapter != null)
                        log.Warning($"Reverted adapter to {preferredAdapter.Name} ({preferredAdapter.ID}) - Previous not found ({selectedID})");
                    else
                        log.Warning($"No adapter available. Previous one not found ({selectedID})");
                }
            }

            // Validate display count.
            if (preferredAdapter != null)
            {
                if (gfxSettings.DisplayOutputIds.Values.Count == 0 ||
                    gfxSettings.DisplayOutputIds.Values.Count > preferredAdapter.Outputs.Count)
                {
                    gfxSettings.DisplayOutputIds.Values.Clear();
                    gfxSettings.DisplayOutputIds.Values.Add(0);
                }
            }

            SelectedAdapter = preferredAdapter;

            gfxSettings.AdapterID.Apply();
            gfxSettings.DisplayOutputIds.Apply();

            return preferredAdapter;
        }

        private void LogAdapter(Logger log, IDisplayAdapter adapter, int index)
        {
            bool hasOutputs = adapter.Outputs.Count > 0;
            log.WriteLine($"   {index++}. {adapter.Name}{(hasOutputs ? " (usable)" : "")}");
            log.WriteLine($"         Type: {adapter.Type}");
            log.WriteLine($"         VRAM: {adapter.Capabilities.DedicatedVideoMemory:N2} MB");
            log.WriteLine($"         Shared VRAM: {adapter.Capabilities.SharedVideoMemory:N2} MB");
            log.WriteLine($"         Dedicated system RAM: {adapter.Capabilities.DedicatedSystemMemory:N2} MB");
            log.WriteLine($"         Shared system RAM: {adapter.Capabilities.SharedSystemMemory:N2} MB");

            index = 1;
            log.WriteLine($"         Command Sets:");
            foreach (SupportedCommandSet set in adapter.Capabilities.CommandSets)
                log.WriteLine($"            {index++}. Limit: {set.MaxCount} -- Capabilities: {set.CapabilityFlags}");

            if (hasOutputs)
            {
                log.WriteLine($"         Detected {adapter.Outputs.Count} outputs:");
                for (int d = 0; d < adapter.Outputs.Count; d++)
                    log.WriteLine($"            Display {d + 1}: {adapter.Outputs[d].Name}");
            }
        }

        /// <summary>Invoked when the current <see cref="DisplayManager"/> is being initialized..</summary>
        /// <param name="settings">An instance of <see cref="GraphicsSettings"/>. This is used to help configure the display manager.</param>
        protected abstract void OnInitialize(Logger log, GraphicsSettings settings);

        /// <summary>
        /// Gets a list of all detected <see cref="IDisplayAdapter"/> on the current machine.
        /// </summary>
        public abstract IReadOnlyList<IDisplayAdapter> Adapters { get; }

        /// <summary>
        /// Adds all adapters that are compatible with the provided <paramref name="capabilities"/>, to the provided output list.
        /// </summary>
        /// <param name="capabilities">The capabilities for which to test.</param>
        /// <param name="adapters">The list in which to add compatibel <see cref="IDisplayAdapter"/> instances.</param>
        public abstract void GetCompatibleAdapters(GraphicsCapabilities capabilities, List<IDisplayAdapter> adapters);

        /// <summary>Gets the system's default display adapter.</summary>
        public abstract IDisplayAdapter DefaultAdapter { get; }

        /// <summary>Gets or sets the adapter currently selected for use by the engine.</summary>
        public abstract IDisplayAdapter SelectedAdapter { get; set; }

        /// <summary>
        /// Attempts to find a <see cref="IDisplayAdapter"/> with an ID matching the one provided.
        /// </summary>
        /// <param name="id">The <see cref="DeviceID"/> to match.</param>
        /// <returns>A matching <see cref="IDisplayAdapter"/>, or null if no match was found.</returns>
        public IDisplayAdapter this[DeviceID id]
        {
            get
            {
                foreach (IDisplayAdapter adapter in Adapters)
                {
                    if (adapter.ID == id)
                        return adapter;
                }

                return null;
            }
        }
    }
}

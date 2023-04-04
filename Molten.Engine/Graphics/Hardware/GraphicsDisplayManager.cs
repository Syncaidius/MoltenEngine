namespace Molten.Graphics
{
    /// <summary>
    /// The base class for an API-specific display manager. This class is responsible for detecting and managing available graphics devices.
    /// </summary>
    public abstract class GraphicsDisplayManager : EngineObject
    {
        /// <summary>Initializes the display manager using the provided <see cref="GraphicsSettings"/>.</summary>
        /// <param name="renderer">The <see cref="RenderService"/> that the display manager will be bound to.</param>
        /// <param name="settings">An instance of <see cref="GraphicsSettings"/>. This is used to help configure the display manager.</param>
        public void Initialize(RenderService renderer, GraphicsSettings settings)
        {
            Log = renderer.Log;
            Settings = settings;

            OnInitialize(settings);

            // Output detection info into renderer log.
            Log.WriteLine($"Detected {Devices.Count} devices:");
            int aID = 1;

            foreach (GraphicsDevice device in Devices)
                LogAdapter(device, aID++);

            GraphicsDevice preferredAdapter = ValidateAdapterSettings(settings);

            // Add all preferred displays to active list
            foreach (int id in settings.DisplayOutputIds.Values)
            {
                if (id < preferredAdapter.Outputs.Count)
                    preferredAdapter.AddActiveOutput(preferredAdapter.Outputs[id]);
            }

            // Log preferred adapter stats
            Log.WriteLine($"Chosen {preferredAdapter.Name}");
        }

        private GraphicsDevice ValidateAdapterSettings(GraphicsSettings gfxSettings)
        {
            // Does the chosen device ID from settings still match any of our detected devices?
            DeviceID selectedID = gfxSettings.AdapterID;
            GraphicsDevice preferredAdapter = this[selectedID];
            if (preferredAdapter == null)
            {
                preferredAdapter = DefaultDevice;
                gfxSettings.AdapterID.Value = preferredAdapter.ID;
                gfxSettings.DisplayOutputIds.Values.Clear();

                if (selectedID.ID != 0)
                {
                    if (preferredAdapter != null)
                        Log.Warning($"Reverted adapter to {preferredAdapter.Name} ({preferredAdapter.ID}) - Previous not found ({selectedID})");
                    else
                        Log.Warning($"No adapter available. Previous one not found ({selectedID})");
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

            SelectedDevice = preferredAdapter;

            gfxSettings.AdapterID.Apply();
            gfxSettings.DisplayOutputIds.Apply();

            return preferredAdapter;
        }

        private void LogAdapter(GraphicsDevice adapter, int index)
        {
            Log.WriteLine($"   {index++}. {adapter.Name}");
            Log.WriteLine($"         Type: {adapter.Type}");
            Log.WriteLine($"         VRAM: {adapter.Capabilities.DedicatedVideoMemory:N2} MB");
            Log.WriteLine($"         Shared VRAM: {adapter.Capabilities.SharedVideoMemory:N2} MB");
            Log.WriteLine($"         Dedicated system RAM: {adapter.Capabilities.DedicatedSystemMemory:N2} MB");
            Log.WriteLine($"         Shared system RAM: {adapter.Capabilities.SharedSystemMemory:N2} MB");

            index = 1;
            Log.WriteLine($"         Command Sets:");
            foreach (SupportedCommandSet set in adapter.Capabilities.CommandSets)
                Log.WriteLine($"            {index++}. Limit: {set.MaxQueueCount} -- Capabilities: {set.CapabilityFlags}");

            if (adapter.Outputs.Count > 0)
            {
                Log.WriteLine($"         Detected {adapter.Outputs.Count} outputs:");
                for (int d = 0; d < adapter.Outputs.Count; d++)
                    Log.WriteLine($"            Display {d + 1}: {adapter.Outputs[d].Name}");
            }
        }

        /// <summary>Invoked when the current <see cref="GraphicsDisplayManager"/> is being initialized..</summary>
        /// <param name="log">The <see cref="Logger"/> for providing feedback or debug info about initialization.</param>
        /// <param name="settings">An instance of <see cref="GraphicsSettings"/>. This is used to help configure the display manager.</param>
        protected abstract void OnInitialize(GraphicsSettings settings);

        /// <summary>
        /// Gets a list of all detected <see cref="GraphicsDevice"/> on the current machine.
        /// </summary>
        public abstract IReadOnlyList<GraphicsDevice> Devices { get; }

        /// <summary>
        /// Adds all adapters that are compatible with the provided <paramref name="capabilities"/>, to the provided output list.
        /// </summary>
        /// <param name="capabilities">The capabilities for which to test.</param>
        /// <param name="adapters">The list in which to add compatibel <see cref="GraphicsDevice"/> instances.</param>
        public abstract void GetCompatibleAdapters(GraphicsCapabilities capabilities, List<GraphicsDevice> adapters);

        /// <summary>Gets the system's default display adapter.</summary>
        public abstract GraphicsDevice DefaultDevice { get; }

        /// <summary>Gets or sets the adapter currently selected for use by the engine.</summary>
        public abstract GraphicsDevice SelectedDevice { get; set; }

        /// <summary>
        /// Gets the <see cref="GraphicsSettings"/> that the current <see cref="GraphicsDisplayManager"/> was initialized and bound to.
        /// </summary>
        public GraphicsSettings Settings { get; private set; }

        /// <summary>
        /// Getts the <see cref="Logger"/> that the current <see cref="GraphicsDisplayManager"/> uses for outputting information.
        /// </summary>
        public Logger Log { get; private set; }

        /// <summary>
        /// Attempts to find a <see cref="GraphicsDevice"/> with an ID matching the one provided.
        /// </summary>
        /// <param name="id">The <see cref="DeviceID"/> to match.</param>
        /// <returns>A matching <see cref="GraphicsDevice"/>, or null if no match was found.</returns>
        public GraphicsDevice this[DeviceID id]
        {
            get
            {
                foreach (GraphicsDevice devices in Devices)
                {
                    if (devices.ID == id)
                        return devices;
                }

                return null;
            }
        }
    }
}

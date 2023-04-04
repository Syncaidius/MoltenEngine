using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Molten.Graphics
{
    internal unsafe class DisplayManagerVK : GraphicsManager
    {
        List<DeviceVK> _devices;
        DeviceVK _defaultAdapter;
        DeviceVK _selectedAdapter;

        internal DisplayManagerVK(RendererVK renderer)
        {
            Renderer = renderer;
            CapBuilder = new CapabilityBuilder();
            _devices = new List<DeviceVK>();
            Devices = _devices.AsReadOnly();
            Outputs = new List<DisplayOutputVK>();
        }

        protected override void OnInitialize(GraphicsSettings settings)
        {
            uint deviceCount = 0;
            Result r = Renderer.VK.EnumeratePhysicalDevices(*Renderer.Instance, &deviceCount, null);

            if (r.Check(Renderer))
            {
                PhysicalDevice* devices = EngineUtil.AllocArray<PhysicalDevice>(deviceCount);
                r = Renderer.VK.EnumeratePhysicalDevices(*Renderer.Instance, &deviceCount, devices); 
                
                if (r.Check(Renderer))
                {
                    for (int i = 0; i < deviceCount; i++)
                    {
                        DeviceVK adapter = new DeviceVK(Renderer, this, devices[0], Renderer.Instance);
                        _devices.Add(adapter);
                    }
                }

                EngineUtil.Free(ref devices);
            }

            Renderer.GLFW.SetMonitorCallback(MonitorConnectionCallback);

            DetectOutputs();

            if (_devices.Count > 0)
            {
                _defaultAdapter = _devices[0];

                // TODO improve this.
                // For now, associate all detected outputs with the default adapter.
                foreach (DisplayOutputVK output in Outputs)
                    _defaultAdapter.AssociateOutput(output);
            }
        }

        private void DetectOutputs()
        {
            int monitorCount = 0;
            Monitor** monitors = Renderer.GLFW.GetMonitors(out monitorCount);
            Monitor* primaryMonitor = Renderer.GLFW.GetPrimaryMonitor();

            for(int i = 0; i < monitorCount; i++)
            {
                Monitor* monitor = monitors[i];
                DisplayOutputVK vkOutput = new DisplayOutputVK(this, monitor);

                if (monitor == primaryMonitor)
                    PrimaryOutput = vkOutput;

                Outputs.Add(vkOutput);
            }
        }

        private void MonitorConnectionCallback(Monitor* monitor, ConnectedState state)
        {
            switch (state)
            {
                case ConnectedState.Connected:
                    {
                        DisplayOutputVK vkOutput = new DisplayOutputVK(this, monitor);
                        Outputs.Add(vkOutput);

                        // Associate with the selected adapter by default.
                        _selectedAdapter?.AssociateOutput(vkOutput);
                        Renderer.Log.WriteLine($"Display output '{vkOutput.Name}' connected");
                    }
                    break;

                case ConnectedState.Disconnected:
                    {
                        foreach (DisplayOutputVK vkOutput in Outputs)
                        {
                            if (vkOutput.Ptr == monitor)
                            {
                                vkOutput.AssociatedDevice?.UnassociateOutput(vkOutput);
                                Outputs.Remove(vkOutput);
                                Renderer.Log.WriteLine($"Display output '{vkOutput.Name}' disconnected");
                            }
                        }
                    }
                    break;
            }
        }

        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }

        public override void GetCompatibleAdapters(GraphicsCapabilities capabilities, List<GraphicsDevice> adapters)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override GraphicsDevice DefaultDevice => _defaultAdapter;

        /// <inheritdoc/>
        public override GraphicsDevice SelectedDevice
        {
            get => _selectedAdapter;
            set
            {
                if (value != null)
                {
                    if (value is not DeviceVK vkDevice)
                        throw new GraphicsDeviceException(value, "The adapter is not a valid Vulkan device.");

                    if (value.Manager != this)
                        throw new GraphicsDeviceException(value, "The adapter not owned by the current display manager.");

                    _selectedAdapter = vkDevice;
                }
                else
                {
                    _selectedAdapter = null;
                }
            }
        }

        internal List<DisplayOutputVK> Outputs { get; }

        internal DisplayOutputVK PrimaryOutput { get; private set; }

        /// <inheritdoc/>
        public override IReadOnlyList<GraphicsDevice> Devices { get; }

        internal RendererVK Renderer { get; }

        internal CapabilityBuilder CapBuilder { get; }
    }
}

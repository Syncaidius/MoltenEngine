using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Vulkan;
using Silk.NET.GLFW;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Molten.Graphics
{
    internal unsafe class DisplayManagerVK : GraphicsDisplayManager
    {
        List<DisplayAdapterVK> _adapters;
        DisplayAdapterVK _defaultAdapter;
        DisplayAdapterVK _selectedAdapter;

        internal DisplayManagerVK(RendererVK renderer)
        {
            Renderer = renderer;
            CapBuilder = new CapabilityBuilder();
            _adapters = new List<DisplayAdapterVK>();
            Adapters = _adapters.AsReadOnly();
            Outputs = new List<DisplayOutputVK>();
        }

        protected override void OnInitialize(Logger log, GraphicsSettings settings)
        {
            uint deviceCount = 0;
            Result r = Renderer.VK.EnumeratePhysicalDevices(*Renderer.Instance, &deviceCount, null);

            if (Renderer.CheckResult(r))
            {
                PhysicalDevice* devices = EngineUtil.AllocArray<PhysicalDevice>(deviceCount);
                r = Renderer.VK.EnumeratePhysicalDevices(*Renderer.Instance, &deviceCount, devices); 
                
                if (Renderer.CheckResult(r))
                {
                    for (int i = 0; i < deviceCount; i++)
                    {
                        DisplayAdapterVK adapter = new DisplayAdapterVK(this, devices[0]);
                        _adapters.Add(adapter);
                    }
                }

                EngineUtil.Free(ref devices);
            }

            Renderer.GLFW.SetMonitorCallback(MonitorConnectionCallback);

            DetectOutputs();

            if (_adapters.Count > 0)
            {
                _defaultAdapter = _adapters[0];

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
                                vkOutput.AssociatedAdapter?.UnassociateOutput(vkOutput);
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

        public override void GetCompatibleAdapters(GraphicsCapabilities capabilities, List<IDisplayAdapter> adapters)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IDisplayAdapter DefaultAdapter => _defaultAdapter;

        /// <inheritdoc/>
        public override IDisplayAdapter SelectedAdapter
        {
            get => _selectedAdapter;
            set
            {
                if (value != null)
                {
                    if (value is not DisplayAdapterVK vkAdapter)
                        throw new AdapterException(value, "The adapter is not a valid Vulkan adapter.");

                    if (value.Manager != this)
                        throw new AdapterException(value, "The adapter not owned by the current display manager.");

                    _selectedAdapter = vkAdapter;
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
        public override IReadOnlyList<IDisplayAdapter> Adapters { get; }

        internal RendererVK Renderer { get; }

        internal CapabilityBuilder CapBuilder { get; }
    }
}

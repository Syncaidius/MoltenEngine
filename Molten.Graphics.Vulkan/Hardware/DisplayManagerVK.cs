using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Monitor = Silk.NET.GLFW.Monitor;

namespace Molten.Graphics.Vulkan;

internal unsafe class DisplayManagerVK : GpuManager
{
    List<DeviceVK> _devices;
    DeviceVK _defaultAdapter;
    DeviceVK _selectedAdapter;

    internal DisplayManagerVK(RendererVK renderer)
    {
        Renderer = renderer;
        CapBuilder = new CapabilityBuilderVK();
        _devices = new List<DeviceVK>();
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
                // Vulkan sometimes returns duplicate devices with the same ID.
                // For now we work around this by using a hashset to test device IDs.
                HashSet<DeviceID> deviceIDs = new HashSet<DeviceID>();

                for (int i = 0; i < deviceCount; i++)
                {
                    DeviceVK adapter = new DeviceVK(Renderer, this, devices[i], Renderer.Instance);

                    if(deviceIDs.Contains(adapter.ID))
                    {
                        adapter.Dispose();
                        continue;
                    }
                    
                    deviceIDs.Add(adapter.ID);
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

    protected override void OnDispose(bool immediate)
    {
        throw new NotImplementedException();
    }

    public override void GetCompatibleAdapters(GpuCapabilities capabilities, List<GpuDevice> adapters)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override GpuDevice DefaultDevice => _defaultAdapter;

    /// <inheritdoc/>
    public override GpuDevice PrimaryDevice
    {
        get => _selectedAdapter;
        set
        {
            if (value != null)
            {
                if (value is not DeviceVK Device)
                    throw new GpuDeviceException(value, "The adapter is not a valid Vulkan device.");

                if (value.Manager != this)
                    throw new GpuDeviceException(value, "The adapter not owned by the current display manager.");

                _selectedAdapter = Device;
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
    public override IReadOnlyList<GpuDevice> Devices => _devices;

    internal RendererVK Renderer { get; }

    internal CapabilityBuilderVK CapBuilder { get; }
}

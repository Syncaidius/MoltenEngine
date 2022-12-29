using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics.Hardware;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class DisplayManagerVK : IDisplayManager
    {
        RendererVK _renderer;
        List<DisplayAdapterVK> _adapters;

        internal DisplayManagerVK(RendererVK renderer)
        {
            _renderer = renderer;
            _adapters = new List<DisplayAdapterVK>();
        }

        public void Initialize(Logger logger, GraphicsSettings settings)
        {
            uint deviceCount = 0;
            Result r = _renderer.VK.EnumeratePhysicalDevices(*_renderer.Ptr, &deviceCount, null);

            CapabilityBuilder capBuilder = new CapabilityBuilder();

            if (_renderer.LogResult(r))
            {
                PhysicalDevice* devices = EngineUtil.AllocArray<PhysicalDevice>(deviceCount);
                r = _renderer.VK.EnumeratePhysicalDevices(*_renderer.Ptr, &deviceCount, devices);
                if (_renderer.LogResult(r))
                {
                    for (int i = 0; i < deviceCount; i++)
                    {
                        PhysicalDeviceProperties2 dProperties;
                        _renderer.VK.GetPhysicalDeviceProperties2(*devices, &dProperties);

                        PhysicalDeviceFeatures2 dFeatures;
                        _renderer.VK.GetPhysicalDeviceFeatures2(*devices, &dFeatures);

                        GraphicsCapabilities cap = capBuilder.Build(ref dProperties.Properties, ref dProperties.Properties.Limits, ref dFeatures.Features);
                        DisplayAdapterVK adapter = new DisplayAdapterVK(this, cap, ref dProperties.Properties);
                        _adapters.Add(adapter);

                        devices++;
                    }
                }

                EngineUtil.Free(ref devices);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void GetCompatibleAdapters(GraphicsCapabilities capabilities, List<IDisplayAdapter> adapters)
        {
            throw new NotImplementedException();
        }

        public IDisplayAdapter DefaultAdapter => throw new NotImplementedException();

        public IDisplayAdapter SelectedAdapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IReadOnlyList<IDisplayAdapter> Adapters => throw new NotImplementedException();

        public IReadOnlyList<IDisplayAdapter> AdaptersWithOutputs => throw new NotImplementedException();

        public IDisplayAdapter this[DeviceID id] => throw new NotImplementedException();
    }
}

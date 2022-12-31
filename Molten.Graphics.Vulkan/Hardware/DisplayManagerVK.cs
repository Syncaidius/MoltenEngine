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
                        PhysicalDeviceProperties2 dProperties = new PhysicalDeviceProperties2(StructureType.PhysicalDeviceProperties2);
                        _renderer.VK.GetPhysicalDeviceProperties2(devices[i], &dProperties);

                        PhysicalDeviceFeatures2 dFeatures = new PhysicalDeviceFeatures2(StructureType.PhysicalDeviceFeatures2);
                        _renderer.VK.GetPhysicalDeviceFeatures2(devices[i], &dFeatures);

                        GraphicsCapabilities cap = capBuilder.Build(ref dProperties, ref dProperties.Properties.Limits, ref dFeatures.Features);
                        DisplayAdapterVK adapter = new DisplayAdapterVK(this, cap, ref dProperties);
                        _adapters.Add(adapter);

                        capBuilder.LogAdditionalProperties(logger, &dProperties);
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

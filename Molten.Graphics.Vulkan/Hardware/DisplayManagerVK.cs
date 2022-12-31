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
        List<DisplayAdapterVK> _adapters;

        internal DisplayManagerVK(RendererVK renderer)
        {
            Renderer = renderer;
            CapBuilder = new CapabilityBuilder();
            _adapters = new List<DisplayAdapterVK>();
        }

        public void Initialize(Logger logger, GraphicsSettings settings)
        {
            uint deviceCount = 0;
            Result r = Renderer.VK.EnumeratePhysicalDevices(*Renderer.Ptr, &deviceCount, null);

            if (Renderer.LogResult(r))
            {
                PhysicalDevice* devices = EngineUtil.AllocArray<PhysicalDevice>(deviceCount);
                r = Renderer.VK.EnumeratePhysicalDevices(*Renderer.Ptr, &deviceCount, devices); 
                
                if (Renderer.LogResult(r))
                {
                    for (int i = 0; i < deviceCount; i++)
                    {
                        DisplayAdapterVK adapter = new DisplayAdapterVK(this, devices[0]);
                        _adapters.Add(adapter);
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

        internal RendererVK Renderer { get; }

        internal CapabilityBuilder CapBuilder { get; }
    }
}

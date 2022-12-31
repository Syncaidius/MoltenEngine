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
        List<DisplayAdapterVK> _withOutputs;

        internal DisplayManagerVK(RendererVK renderer)
        {
            Renderer = renderer;
            CapBuilder = new CapabilityBuilder();
            _adapters = new List<DisplayAdapterVK>();
            Adapters = _adapters.AsReadOnly();
            _withOutputs = new List<DisplayAdapterVK>();
            AdaptersWithOutputs = _withOutputs.AsReadOnly();
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

        /// <inheritdoc/>
        public IDisplayAdapter DefaultAdapter => throw new NotImplementedException();

        /// <inheritdoc/>
        public IDisplayAdapter SelectedAdapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayAdapter> Adapters { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayAdapter> AdaptersWithOutputs { get; }

        /// <inheritdoc/>
        public IDisplayAdapter this[DeviceID id] => throw new NotImplementedException();

        internal RendererVK Renderer { get; }

        internal CapabilityBuilder CapBuilder { get; }
    }
}

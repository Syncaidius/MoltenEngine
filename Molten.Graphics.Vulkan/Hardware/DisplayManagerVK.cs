using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class DisplayManagerVK : IDisplayManager
    {
        List<DisplayAdapterVK> _adapters;
        List<DisplayAdapterVK> _withOutputs;
        DisplayAdapterVK _defaultAdapter;
        DisplayAdapterVK _selectedAdapter;

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
            Result r = Renderer.VK.EnumeratePhysicalDevices(*Renderer.Instance.Ptr, &deviceCount, null);

            if (Renderer.LogResult(r))
            {
                PhysicalDevice* devices = EngineUtil.AllocArray<PhysicalDevice>(deviceCount);
                r = Renderer.VK.EnumeratePhysicalDevices(*Renderer.Instance.Ptr, &deviceCount, devices); 
                
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

            // TODO set the adapter with the highest resolution display/output as the default
            if (_adapters.Count > 0)
                _defaultAdapter = _adapters[0];
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
        public IDisplayAdapter DefaultAdapter => _defaultAdapter;

        /// <inheritdoc/>
        public IDisplayAdapter SelectedAdapter
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

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayAdapter> Adapters { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayAdapter> AdaptersWithOutputs { get; }

        /// <inheritdoc/>
        public IDisplayAdapter this[DeviceID id]
        {
            get
            {
                foreach (IDisplayAdapter adapter in _adapters)
                {
                    if (adapter.ID == id)
                        return adapter;
                }

                return null;
            }
        }

        internal RendererVK Renderer { get; }

        internal CapabilityBuilder CapBuilder { get; }
    }
}

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
        public int AdapterCount => throw new NotImplementedException();

        public IDisplayAdapter DefaultAdapter => throw new NotImplementedException();

        public IDisplayAdapter SelectedAdapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        RendererVK _renderer;

        internal DisplayManagerVK(RendererVK renderer)
        {
            _renderer = renderer;
        }

        public void Initialize(Logger logger, GraphicsSettings settings)
        {
            uint deviceCount = 0;
            Result r = _renderer.VK.EnumeratePhysicalDevices(*_renderer.Ptr, &deviceCount, null);

            if (_renderer.LogResult(r))
            {
                PhysicalDevice* devices = EngineUtil.AllocArray<PhysicalDevice>(deviceCount);
                r = _renderer.VK.EnumeratePhysicalDevices(*_renderer.Ptr, &deviceCount, devices);
                if (_renderer.LogResult(r))
                {
                    for (int i = 0; i < deviceCount; i++)
                    {
                        PhysicalDeviceProperties dProperties;
                        _renderer.VK.GetPhysicalDeviceProperties(*devices, &dProperties);
                        GraphicsCapabilities cap = new GraphicsCapabilities();
                        PopulateCapabilityLimits(cap, ref dProperties.Limits);

                        PhysicalDeviceFeatures dFeatures;
                        _renderer.VK.GetPhysicalDeviceFeatures(*devices, &dFeatures);
                        PopulateCapabilityFeatures(cap, ref dFeatures);

                        devices++;
                    }
                }
            }
        }

        private void PopulateCapabilityLimits(GraphicsCapabilities cap, ref PhysicalDeviceLimits limits)
        {
            cap.MaxTexture1DDimension = limits.MaxImageDimension1D;
            cap.MaxTexture2DDimension = limits.MaxImageDimension2D;
            cap.MaxTexture3DDimension = limits.MaxImageDimension3D;
            cap.MaxTextureCubeDimension = limits.MaxImageDimensionCube;
        }

        private void PopulateCapabilityFeatures(GraphicsCapabilities cap, ref PhysicalDeviceFeatures features)
        {
            cap.HullShader.IsSupported = features.TessellationShader;
            cap.DomainShader.IsSupported = features.TessellationShader;
            cap.GeometryShader.IsSupported = features.GeometryShader;
         
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IDisplayAdapter GetAdapter(int id)
        {
            throw new NotImplementedException();
        }

        public void GetAdapters(List<IDisplayAdapter> output)
        {
            throw new NotImplementedException();
        }

        public void GetAdaptersWithOutputs(List<IDisplayAdapter> output)
        {
            throw new NotImplementedException();
        }
    }
}

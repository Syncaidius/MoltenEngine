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
            bool int16 = features.ShaderInt16;
            bool int64 = features.ShaderInt64;
            bool float64 = features.ShaderFloat64;

            SetShaderPrecision(cap.VertexShader, true, int16, int64, float64);
            SetShaderPrecision(cap.GeometryShader, features.GeometryShader, int16, int64, float64);
            SetShaderPrecision(cap.HullShader, features.TessellationShader, int16, int64, float64);
            SetShaderPrecision(cap.DomainShader, features.TessellationShader, int16, int64, float64);
            SetShaderPrecision(cap.PixelShader, true, int16, int64, float64);
            SetShaderPrecision(cap.Compute, true, int16, int64, float64);            
        }

        private void SetShaderPrecision(ShaderStageCapabilities sCap, bool supported, bool int16, bool int64, bool float64)
        {
            sCap.IsSupported = supported;
            sCap.Int16 = int16;
            sCap.Int64 = int64;
            sCap.Float64 = float64;
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

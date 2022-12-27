using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class CapabilityBuilder
    {
        internal GraphicsCapabilities Build(ref PhysicalDeviceLimits limits, ref PhysicalDeviceFeatures features)
        {
            GraphicsCapabilities cap = new GraphicsCapabilities();
            PopulateCapabilityLimits(cap, ref limits);
            PopulateCapabilityFeatures(cap, ref features);

            cap.Sampler.MaxAnisotropy = features.SamplerAnisotropy ? limits.MaxSamplerAnisotropy : 0;
            cap.MaxArraySlices = limits.MaxImageArrayLayers;
            cap.TextureCubeArraySupport = features.ImageCubeArray;

            return cap;
        }

        private void PopulateCapabilityLimits(GraphicsCapabilities cap, ref PhysicalDeviceLimits limits)
        {
            cap.MaxTexture1DSize = limits.MaxImageDimension1D;
            cap.MaxTexture2DSize = limits.MaxImageDimension2D;
            cap.MaxTexture3DSize = limits.MaxImageDimension3D;
            cap.MaxTextureCubeSize = limits.MaxImageDimensionCube;
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
    }
}

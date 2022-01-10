using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feature = Silk.NET.Direct3D11.Feature;

namespace Molten.Graphics
{
    public class GraphicsComputeFeatures
    {
        DeviceFeaturesDX11 _features;

        internal GraphicsComputeFeatures(DeviceFeaturesDX11 features)
        {
            _features = features;

            MaxThreadGroupSize = 1024;
            MaxThreadGroupZ = 64;
            MaxDispatchXYDimension = 65535;
            MaxDispatchZDimension = 65535;

            FeatureDataD3D10XHardwareOptions fData =
                _features.GetFeatureSupport<FeatureDataD3D10XHardwareOptions>(Feature.FeatureD3D10XHardwareOptions);

            Supported = fData.ComputeShadersPlusRawAndStructuredBuffersViaShader4X > 0;
        }

        /// <summary>Returns all of the supported compute shader features for the provided <see cref="Format"/>.</summary>
        /// <param name="format">The format of which to retrieve compute shader support.</param>
        /// <returns>Returns <see cref="FormatSupport2"/> flags containing compute feature support for the specified <see cref="Format"/>.</returns>
        public unsafe FormatSupport2 GetFormatSupport(Format format)
        {
            FeatureDataFormatSupport2 pData = new FeatureDataFormatSupport2()
            {
                InFormat = format,
            };

            _features.GetFeatureSupport(Feature.FeatureFormatSupport2, &pData);

            return (FormatSupport2)pData.OutFormatSupport2;
        }

        /// <summary>Gets the maximum supported size of a compute shader thread group.</summary>
        public int MaxThreadGroupSize { get; }

        /// <summary>Gets the maximum z dimension of a compute shader thread group.</summary>
        public int MaxThreadGroupZ { get; }

        /// <summary>Gets the maximum size that a dispatch dimension can be.</summary>
        public int MaxDispatchXYDimension { get; }

        /// <summary>Gets the max dispatch size of the Z dimension.</summary>
        public int MaxDispatchZDimension { get; }

        /// <summary>Gets whether or not compute shaders are supported.</summary>
        public bool Supported { get; }
    }
}

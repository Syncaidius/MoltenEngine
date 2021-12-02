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
    public unsafe class GraphicsComputeFeatures
    {
        ID3D11Device* _device;

        internal GraphicsComputeFeatures(ID3D11Device* device)
        {
            _device = device;
        }

        /// <summary>Returns all of the supported compute shader features for the provided DXGI format.</summary>
        /// <param name="format">The format of which to retrieve compute shader support.</param>
        /// <returns></returns>
        public FormatSupport2 GetComputeShaderSupport(Format format)
        {
            FeatureDataFormatSupport2 supportData = new FeatureDataFormatSupport2()
            {
                InFormat = format,
            };

            _device->CheckFeatureSupport(Feature.FeatureFormatSupport2,
                &supportData,
                (uint)sizeof(FeatureDataFormatSupport2));

            return (FormatSupport2)supportData.OutFormatSupport2;
        }

        /// <summary>Gets the maximum supported size of a compute shader thread group.</summary>
        public int MaxThreadGroupSize { get; internal set; }

        /// <summary>Gets the maximum z dimension of a compute shader thread group.</summary>
        public int MaxThreadGroupZ { get; internal set; }

        /// <summary>Gets the maximum size that a dispatch dimension can be.</summary>
        public int MaxDispatchXYDimension { get; internal set; }

        /// <summary>Gets the max dispatch size of the Z dimension.</summary>
        public int MaxDispatchZDimension { get; internal set; }

        /// <summary>Gets whether or not compute shaders are supported.</summary>
        public bool Supported { get; internal set; }
    }
}

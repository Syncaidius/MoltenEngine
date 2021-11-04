using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class GraphicsComputeFeatures
    {
        Device _d3d;

        internal GraphicsComputeFeatures(Device d3d)
        {
            _d3d = d3d;
        }

        /// <summary>Returns all of the supported compute shader features for the provided DXGI format.</summary>
        /// <param name="format">The format of which to retrieve compute shader support.</param>
        /// <returns></returns>
        public ComputeShaderFormatSupport GetComputeShaderSupport(SharpDX.DXGI. Format format)
        {
            return _d3d.CheckComputeShaderFormatSupport(format);
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

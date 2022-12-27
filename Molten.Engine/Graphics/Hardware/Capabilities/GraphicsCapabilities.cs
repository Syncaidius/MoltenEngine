using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class GraphicsCapabilities
    {       
        /// <summary>Gets the maximum size of a 1D texture. A size of 2048 would mean a maximum 1D texture size of 1 x 2048.</summary>
        public uint MaxTexture1DSize { get; set; }

        /// <summary>Gets the maximum size of a 1D texture. A size of 2048 would mean a maximum 2D texture size of 2048 x 2048.</summary>
        public uint MaxTexture2DSize { get; set; }

        /// <summary>Gets the maximum size of a 3D texture. A size of 128 would mean a maximum 3D texture size of 128 x 128 x 128.</summary>
        public uint MaxTexture3DSize { get; set; }

        /// <summary>Gets the maximum size of a cube texture. A size of 128 would mean a maximum 3D texture size of 128 x 128 x 128.</summary>
        public uint MaxTextureCubeSize { get; set; }

        /// <summary>Gets whether or not texture cube arrays are supported.</summary>
        public bool TextureCubeArraySupport { get; set; }

        /// <summary>
        /// Gets the maximum number of array slices a texture array can have.
        /// </summary>
        public uint MaxArraySlices { get; set; }

        public ShaderStageCapabilities VertexShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities PixelShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities GeometryShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities HullShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities DomainShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities Compute { get; } = new ShaderStageCapabilities();

        /// <summary>
        /// Gets sampler capabilities.
        /// </summary>
        public SamplerCapabilities Sampler { get; } = new SamplerCapabilities();

        public bool Compatible(GraphicsCapabilities other)
        {
            // TODO compare current to other. Current must have at least everything 'other' specifies.

            return true;
        }
    }
}

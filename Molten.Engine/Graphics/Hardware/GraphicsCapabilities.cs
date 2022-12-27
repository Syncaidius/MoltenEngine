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
        public uint MaxTexture1DDimension { get; set; }

        /// <summary>Gets the maximum size of a 1D texture. A size of 2048 would mean a maximum 2D texture size of 2048 x 2048.</summary>
        public uint MaxTexture2DDimension { get; set; }

        /// <summary>Gets the maximum size of a 3D texture. A size of 128 would mean a maximum 3D texture size of 128 x 128 x 128.</summary>
        public uint MaxTexture3DDimension { get; set; }

        /// <summary>Gets the maximum size of a cube texture. A size of 128 would mean a maximum 3D texture size of 128 x 128 x 128.</summary>
        public uint MaxTextureCubeDimension { get; set; }

        public ShaderStageCapabilities VertexShader { get; } = new ShaderStageCapabilities(true);

        public ShaderStageCapabilities PixelShader { get; } = new ShaderStageCapabilities(true);

        public ShaderStageCapabilities GeometryShader { get; } = new ShaderStageCapabilities(true);

        public ShaderStageCapabilities HullShader { get; } = new ShaderStageCapabilities(true);

        public ShaderStageCapabilities DomainShader { get; } = new ShaderStageCapabilities(true);

        public ShaderStageCapabilities Compute { get; } = new ShaderStageCapabilities(true);

        public bool Compatible(GraphicsCapabilities other)
        {
            // TODO compare current to other. Current must have at least everything 'other' specifies.

            return true;
        }
    }

    public class ShaderStageCapabilities
    {
        public bool Supported { get; set; }

        internal ShaderStageCapabilities(bool supported)
        {
            Supported = supported;
        }

        public bool Compatible(ShaderStageCapabilities other)
        {
            // TODO compare current to other. Current must have at least everything 'other' specifies.

            return true;
        }
    }
}

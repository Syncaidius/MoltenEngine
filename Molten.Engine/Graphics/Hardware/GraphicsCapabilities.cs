﻿using System;
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

        public ShaderStageCapabilities VertexShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities PixelShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities GeometryShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities HullShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities DomainShader { get; } = new ShaderStageCapabilities();

        public ShaderStageCapabilities Compute { get; } = new ShaderStageCapabilities();

        public bool Compatible(GraphicsCapabilities other)
        {
            // TODO compare current to other. Current must have at least everything 'other' specifies.

            return true;
        }
    }

    public class ShaderStageCapabilities
    {
        /// <summary>
        /// Gets or sets whether the shader stage is supported.
        /// </summary>
        public bool IsSupported { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the use of 10-bit-precision floating-point values is supported by the shader stage.
        /// </summary>
        public bool Float10 { get; set; }

        /// <summary>
        /// Gets or sets whether the use of 16-bit integer values is supported by the shader stage.
        /// </summary>
        public bool Int16 { get; set; }

        /// <summary>
        /// Gets or sets whether the use of 6-bit half-precision floating-point values is supported by the shader stage.
        /// </summary>
        public bool Float16 { get; set; }

        /// <summary>
        /// Gets or sets whether the use of 64-bit integer values is supported by the shader stage.
        /// </summary>
        public bool Int64 { get; set; }

        /// <summary>
        /// Gets or sets whether the use of 64-bit, double-precision floating-point values is supported by the shader stage.
        /// </summary>
        public bool Float64 { get; set; }

        public bool Compatible(ShaderStageCapabilities other)
        {
            // TODO compare current to other. Current must have at least everything 'other' specifies.

            return true;
        }
    }
}

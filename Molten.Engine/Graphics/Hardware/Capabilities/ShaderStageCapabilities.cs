using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
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

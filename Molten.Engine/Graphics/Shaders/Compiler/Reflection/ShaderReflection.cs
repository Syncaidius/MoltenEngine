using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ShaderReflection
    {
        /// <summary>
        /// Gets the primitive topology that is expected as input for the geometry shader stage.
        /// </summary>
        public PrimitiveTopology GSInputPrimitive;

        public List<ShaderResourceInfo> BoundResources { get; } = new List<ShaderResourceInfo>();

        public List<ShaderParameterInfo> InputParameters { get; } = new List<ShaderParameterInfo>();

        public List<ShaderParameterInfo> OutputParameters { get; } = new List<ShaderParameterInfo>();

        public Dictionary<string, ConstantBufferInfo> ConstantBuffers { get; } = new Dictionary<string, ConstantBufferInfo>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ShaderComposition
    {
        /// <summary>A list of const buffers the shader stage requires to be bound.</summary>
        internal List<int> ConstBufferIds = new List<int>();

        /// <summary>A list of resources that must be bound to the shader stage.</summary>
        internal List<int> ResourceIds = new List<int>();

        /// <summary>A list of samplers that must be bound to the shader stage.</summary>
        internal List<int> SamplerIds = new List<int>();

        internal List<int> UnorderedAccessIds = new List<int>();

        internal ShaderIOStructure InputStructure;

        internal ShaderIOStructure OutputStructure;

        internal string EntryPoint;

        internal bool Optional;

        internal ShaderComposition(bool optional)
        {
            Optional = optional;
        }
    }

    internal class ShaderComposition<T> : ShaderComposition where T : DeviceChild
    {
        internal ShaderComposition(bool optional) : base(optional) { }

        /// <summary>The underlying, compiled HLSL shader object.</summary>
        internal T RawShader;
    }
}

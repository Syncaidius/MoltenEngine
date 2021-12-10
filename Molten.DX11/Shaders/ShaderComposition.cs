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
        internal List<uint> ConstBufferIds = new List<uint>();

        /// <summary>A list of resources that must be bound to the shader stage.</summary>
        internal List<uint> ResourceIds = new List<uint>();

        /// <summary>A list of samplers that must be bound to the shader stage.</summary>
        internal List<uint> SamplerIds = new List<uint>();

        internal List<uint> UnorderedAccessIds = new List<uint>();

        internal ShaderIOStructure InputStructure;

        internal ShaderIOStructure OutputStructure;

        internal string EntryPoint;

        internal bool Optional;

        internal ShaderComposition(bool optional)
        {
            Optional = optional;
        }
    }

    internal unsafe class ShaderComposition<T> : ShaderComposition 
        where T : unmanaged
    {
        internal ShaderComposition(bool optional) : base(optional) { }

        /// <summary>The underlying, compiled HLSL shader object.</summary>
        internal T* RawShader;
    }
}

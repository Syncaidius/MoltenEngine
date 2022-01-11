using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class ShaderComposition : PipeBindable
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

        internal HlslShader Parent { get; }

        internal ShaderComposition(HlslShader parentShader, bool optional) : base(parentShader.Device)
        {
            Parent = parentShader;
            Optional = optional;
        }

        protected internal override void Refresh(PipeSlot slot, PipeDX11 pipe) { }
    }

    internal unsafe class ShaderComposition<T> : ShaderComposition 
        where T : unmanaged
    {
        internal ShaderComposition(HlslShader parentShader, bool optional) : 
            base(parentShader, optional) { }

        /// <summary>The underlying, compiled HLSL shader object.</summary>
        internal T* RawShader;

        internal override void PipelineDispose()
        {
            SilkUtil.ReleasePtr(ref RawShader);
        }
    }
}

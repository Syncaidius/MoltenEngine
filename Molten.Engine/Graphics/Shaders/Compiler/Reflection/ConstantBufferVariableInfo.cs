using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public unsafe class ConstantBufferVariableInfo
    {
        public void* DefaultValue;

        /// <summary>
        /// The offset from the start of the parent constant-buffer, in bytes.
        /// </summary>
        public uint StartOffset;

        /// <summary>
        /// The size of the variable value in bytes.
        /// </summary>
        public uint Size;

        /// <summary>
        /// THe index of the first sampler within the parent buffer.
        /// </summary>
        public uint StartSampler;

        /// <summary>
        /// The size of the sampler, in bytes.
        /// </summary>
        public uint SamplerSize;

        public uint StartTexture;

        /// <summary>
        /// The size of the texture in bytes.
        /// </summary>
        public uint TextureSize;

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name;

        public ShaderVariableFlags Flags;

        public ShaderTypeInfo Type { get; } = new ShaderTypeInfo();
    }
}

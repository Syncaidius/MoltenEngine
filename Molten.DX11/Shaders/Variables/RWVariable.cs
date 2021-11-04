using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class RWVariable : ShaderResourceVariable
    {
        PipelineShaderObject _uResource;

        internal RWVariable(HlslShader shader) : base(shader) { }

        protected sealed override PipelineShaderObject OnSetResource(object value)
        {
            _uResource = OnSetUnorderedResource(value);

            return _uResource;
        }

        protected abstract PipelineShaderObject OnSetUnorderedResource(object value);

        /// <summary>Gets the unordered access version of the resource stored in the variable (e.g. a RWBuffer or RWTexture).</summary>
        public PipelineShaderObject UnorderedResource { get { return _uResource; } }
    }
}

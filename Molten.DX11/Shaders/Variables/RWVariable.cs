using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class RWVariable : ShaderResourceVariable
    {
        PipeBindableResource _uResource;

        internal RWVariable(HlslShader shader) : base(shader) { }

        protected sealed override PipeBindableResource OnSetResource(object value)
        {
            _uResource = OnSetUnorderedResource(value);

            return _uResource;
        }

        protected abstract PipeBindableResource OnSetUnorderedResource(object value);

        /// <summary>Gets the unordered access version of the resource stored in the variable (e.g. a RWBuffer or RWTexture).</summary>
        public PipeBindableResource UnorderedResource { get { return _uResource; } }
    }
}

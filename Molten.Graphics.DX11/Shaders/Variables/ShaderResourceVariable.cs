using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal abstract class ShaderResourceVariable : IShaderValue
    {
        PipeBindableResource _resource;

        internal ShaderResourceVariable(HlslShader shader)
        {
            Parent = shader;
        }

        protected abstract PipeBindableResource OnSetResource(object value);

        /// <summary>Gets the resource bound to the variable.</summary>
        internal PipeBindableResource Resource { get { return _resource; } }

        public string Name { get; set; }

        public IShader Parent { get; private set; }

        public object Value
        {
            get { return Resource; }
            set
            {
                _resource = OnSetResource(value);
            }
        }
    }
}

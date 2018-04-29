using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A helper class for storing references to common shader properties, or filling missing ones in with dummy properties.
    /// </summary>
    internal abstract class CommonShaderProperties
    {
        internal CommonShaderProperties(HlslShader shader) { }

        protected IShaderValue MapValue(HlslShader shader, string name)
        {
            return shader[name] ?? new DummyShaderValue(shader);
        }
    }
}

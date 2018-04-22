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
        internal CommonShaderProperties(Material material) { }

        protected IShaderValue MapValue(Material material, string name)
        {
            return material[name] ?? new DummyShaderValue(material);
        }
    }
}

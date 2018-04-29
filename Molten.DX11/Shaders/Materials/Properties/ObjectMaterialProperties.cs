using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ObjectMaterialProperties : CommonShaderProperties
    {
        internal IShaderValue World { get; set; }

        internal IShaderValue Wvp { get; set; }

        internal ObjectMaterialProperties(Material material) : base(material)
        {
            World = MapValue(material, "world");
            Wvp = MapValue(material, "wvp");
        }
    }
}

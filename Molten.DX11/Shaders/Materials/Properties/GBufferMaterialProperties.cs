using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GBufferMaterialProperties : CommonShaderProperties
    {
        internal IShaderValue EmissivePower { get; set; }

        internal GBufferMaterialProperties(Material material) : base(material)
        {
            EmissivePower = MapValue(material, "emissivePower");
        }
    }
}

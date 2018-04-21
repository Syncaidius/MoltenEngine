using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class SpriteBatchMaterialProperties
    {
        internal IShaderValue EmissivePower { get; set; }

        internal SpriteBatchMaterialProperties(Material material)
        {
            EmissivePower = material["emissivePower"];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ObjectMaterialProperties
    {
        internal IShaderValue World { get; set; }

        internal IShaderValue Wvp { get; set; }

        internal ObjectMaterialProperties(Material material)
        {
            World = material["world"];
            Wvp = material["wvp"];
        }
    }
}

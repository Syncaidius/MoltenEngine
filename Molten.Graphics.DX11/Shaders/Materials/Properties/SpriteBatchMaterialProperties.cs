using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class SpriteBatchMaterialProperties : CommonShaderProperties
    {
        internal IShaderValue TextureSize { get; set; }

        internal SpriteBatchMaterialProperties(Material material) : base(material)
        {
            TextureSize = MapValue(material, "textureSize");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GBufferTextureProperties : CommonShaderProperties
    {
        internal IShaderValue DiffuseTexture { get; set; }

        internal IShaderValue NormalTexture { get; set; }

        internal IShaderValue EmissiveTexture { get; set; }

        internal GBufferTextureProperties(Material material)  : base(material)
        {
            DiffuseTexture = MapValue(material, "mapDiffuse");
            NormalTexture = MapValue(material, "mapNormal");
            EmissiveTexture = MapValue(material, "mapEmissive");
        }
    }
}

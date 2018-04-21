using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class GBufferTextureProperties
    {
        internal IShaderValue DiffuseTexture { get; set; }

        internal IShaderValue NormalTexture { get; set; }

        internal IShaderValue EmissiveTexture { get; set; }

        internal GBufferTextureProperties(Material material)
        {
            DiffuseTexture = material["mapDiffuse"];
            NormalTexture = material["mapNormal"];
            EmissiveTexture = material["mapEmissive"];
        }
    }
}

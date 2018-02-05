using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct TexturegenMipMaps : ITextureChange
    {
        public void Process(GraphicsPipe pipe, TextureBase texture)
        {
            texture.GenerateMipMaps(pipe);
        }
    }
}

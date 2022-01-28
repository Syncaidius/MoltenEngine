using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct TexturegenMipMaps : ITextureTask
    {
        public bool Process(DeviceContext pipe, TextureBase texture)
        {
            texture.GenerateMipMaps(pipe);
            return true;
        }
    }
}

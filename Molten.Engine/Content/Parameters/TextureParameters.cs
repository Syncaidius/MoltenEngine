using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten
{
    public class TextureParameters : IContentParameters
    {
        public uint ArraySize = 1;

        public bool GenerateMipmaps = false;

        public DDSFormat? BlockCompressionFormat = null;
    }
}

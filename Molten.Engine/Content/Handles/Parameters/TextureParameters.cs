using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten
{
    public class TextureParameters : ContentParameters
    {
        public bool GenerateMipmaps = false;

        public DDSFormat? BlockCompressionFormat = null;

        public override object Clone()
        {
            return new TextureParameters()
            {
                GenerateMipmaps = GenerateMipmaps,
                BlockCompressionFormat = BlockCompressionFormat,
                PartCount = PartCount
            };
        }
    }
}

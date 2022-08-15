using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct CharData
    {
        public ushort GlyphIndex;

        public bool Initialized;

        public CharData(ushort gIndex)
        {
            GlyphIndex = gIndex;
            Initialized = true;
        }
    }
}

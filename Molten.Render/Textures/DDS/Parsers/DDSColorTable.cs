using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures.DDS.Parsers
{
    public struct DDSColorTable
    {
        public Color[] color;
        public ushort[] rawColor;
        public uint data;
    }
}

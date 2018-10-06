using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    public struct DXTColorTable
    {
        public Color[] color;
        public ushort[] rawColor;
        public uint data;
    }
}

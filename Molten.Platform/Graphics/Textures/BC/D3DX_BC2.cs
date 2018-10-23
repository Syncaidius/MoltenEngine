using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    internal class D3DX_BC2
    {
        public uint[] bitmap = new uint[2];         // 4bpp alpha bitmap
        public D3DX_BC1 bc1 = new D3DX_BC1();        // BC1 rgb data
    };

}

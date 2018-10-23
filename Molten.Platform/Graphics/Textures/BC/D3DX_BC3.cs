using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Textures
{
    internal class D3DX_BC3
    {
        public byte[] alpha = new byte[2];                 // Alpha values.
        public byte[] bitmap = new byte[6];         // 3bpp alpha bitmap
        public D3DX_BC1 bc1 = new D3DX_BC1();       // BC1 rgb data
    };

}

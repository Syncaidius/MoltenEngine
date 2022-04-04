using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public unsafe struct X2
    {
        public fixed double Values[2];

        public ref double this[int index] => ref Values[index];
    }
}

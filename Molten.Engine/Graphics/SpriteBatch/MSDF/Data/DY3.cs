using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public unsafe struct DY3
    {
        public fixed int Values[3];

        public ref int this[int index] => ref Values[index];
    }
}

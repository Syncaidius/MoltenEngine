using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public unsafe struct X3
    {
        public fixed double Values[3];

        public ref double this[int index] => ref Values[index];
    }
}

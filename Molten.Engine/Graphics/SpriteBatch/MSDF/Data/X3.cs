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

        public static implicit operator X2(X3 value)
        {
            X2 v = new X2();
            v[0] = value[0];
            v[1] = value[1];
            return v;
        }
    }
}

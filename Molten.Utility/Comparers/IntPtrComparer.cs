using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Comparers
{
    public class IntPtrComparer : IComparer<IntPtr>
    {
        public int Compare(IntPtr x, IntPtr y)
        {
            long ix = x.ToInt64();
            long iy = y.ToInt64();

            if (ix < iy)
                return -1;
            else if (ix > iy)
                return 1;
            else
                return 0;
        }
    }
}

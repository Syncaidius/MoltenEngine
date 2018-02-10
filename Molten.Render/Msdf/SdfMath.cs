using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msdfgen
{
    public static class SdfMath
    {
        /// <summary>
        /// Clamps the number to the interval from 0 to b.
        /// </summary>
        /// <returns></returns>
        public static int Clamp(int n, int b)
        {
            if (n > 0)
            {
                return (n <= b) ? n : b;
            }
            return 0;
        }

        /// <summary>
        /// Returns 1 for positive values, -1 for negative values, and 0 for zero.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int Sign(double n)
        {
            return (n == 0) ? 0 : (n > 0) ? 1 : -1;
        }
    }
}

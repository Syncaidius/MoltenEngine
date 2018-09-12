using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>
    /// Provides helper methods for bitwise operations.
    /// </summary>
    public static class BitwiseHelper
    {
        public static void Set(ref int val, params int[] bitIDs)
        {
            for (int i = 0; i < bitIDs.Length; i++)
                val |= (1 << bitIDs[i]);
        }

        public static void Unset(ref int val, params int[] bitIDs)
        {
            for(int i = 0; i < bitIDs.Length; i++)
                val &= ~(1 << bitIDs[i]);
        }

        public static int Set(int val, params int[] bitIDs)
        {
            for (int i = 0; i < bitIDs.Length; i++)
                val |= (1 << bitIDs[i]);

            return val;
        }

        public static int Unset(int val, params int[] bitIDs)
        {
            for (int i = 0; i < bitIDs.Length; i++)
                val &= ~(1 << bitIDs[i]);

            return val;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public static class ArrayExtensions
    {
       public static int IndexOf<T>(this T[] array, T obj)
        {
            return Array.IndexOf(array, obj);
        }

        public static bool Contains<T>(this T[] array, T obj)
        {
            return Array.IndexOf(array, obj) > -1;
        }
    }
}

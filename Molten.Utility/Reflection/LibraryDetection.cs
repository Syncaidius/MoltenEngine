using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public static class LibraryDetection
    {
        public static Assembly LoadLibary(string assemblyPath)
        {            
            return Assembly.LoadFrom(assemblyPath);
        }

        public static IEnumerable<Type> FindType<T>(Assembly assembly)
        {
            Type t = typeof(T);
            Type[] types = assembly.GetTypes();
            types = types.Where(i => t.IsAssignableFrom(i)).ToArray();
            return types;
        }
    }
}

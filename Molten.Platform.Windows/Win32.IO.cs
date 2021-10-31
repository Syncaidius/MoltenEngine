using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Utilities
{
    public class Win32IO
    {
        internal Win32IO() { }

        public string GetShortPathName(string longPath)
        {
            StringBuilder shortPath = new StringBuilder(longPath.Length + 1);

            if (0 == GetShortPathName(longPath, shortPath, shortPath.Capacity))
            {
                return longPath;
            }

            return shortPath.ToString();
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(string path, StringBuilder shortPath, int shortPathLength);
    }
}

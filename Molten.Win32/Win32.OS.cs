using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Utilities
{
    public class Win32OS
    {
        internal Win32OS() { }

        /// <summary>Gets the friendly name of the OS the application is currently running on.</summary>
        /// <returns></returns>
        public string GetName()
        {
            return Win32.GetValue<string>("Win32_OperatingSystem", "Caption");
        }
    }
}

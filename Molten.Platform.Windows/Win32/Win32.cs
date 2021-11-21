using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace Molten.Windows32
{
    /// <summary>A Win32 helper library.</summary>
    public static partial class Win32
    {
        public static Win32CPU CPU { get; private set; } = new Win32CPU();

        public static Win32OS OS { get; private set; } = new Win32OS();

        public static Win32IO IO { get; private set; } = new Win32IO();

        /// <summary>Brings a window to the front and makes it the active/focused one.</summary>
        /// <param name="hwnd">The handle/pointer to a window.</param>
        /// <returns></returns>
        [DllImport("User32")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>Gets the currently focused control.</summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx result);

        static ManagementObjectSearcher _searcher;
        static string _strQuery = "SELECT {0} FROM {1}";

        static Win32()
        {
            _searcher = new ManagementObjectSearcher();
            _searcher.Query = new ObjectQuery()
            {
                QueryLanguage = "WQL",
            };
        }

        /// <summary>Helper method for retrieving key-values from Win32.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetValue<T>(string key, string value)
        {
            T result = default(T);
            _searcher.Query.QueryString = string.Format(_strQuery, value, key);

            //ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT " + value + " FROM " + key);
            try
            {
                foreach (ManagementObject mo in _searcher.Get())
                {
                    result = (T)mo[value];
                    break;
                }
            }
            catch { }

            return result;
        }

        //public static void SetForegroundWindow(WinformsOutput output)
        //{
        //    SetForegroundWindow(output.Form.Handle);
        //}
    }
}

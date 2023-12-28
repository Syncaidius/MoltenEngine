using System.Management;
using System.Runtime.InteropServices;

namespace Molten.Windows32
{
    /// <summary>A Win32 helper library.</summary>
    public static partial class Win32
    {
        public const int SM_MOUSEPRESENT = 19;

        public static Win32CPU CPU { get; } = new Win32CPU();

        public static Win32OS OS { get; } = new Win32OS();

        public static Win32IO IO { get; } = new Win32IO();

        public static Win32Events Events { get; } = new Win32Events();  

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx result);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetSystemMetrics(int nIndex);

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
    }
}

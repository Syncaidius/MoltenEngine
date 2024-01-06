using System.Runtime.InteropServices;
using System.Text;

namespace Molten;

public static class StringHelper
{
    public static byte[] GetBytes(object o)
    {
        int size = Marshal.SizeOf(o);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(o, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    public static byte[] GetBytes(string str, Encoding encoding)
    {
        return encoding.GetBytes(str); ;
    }
}

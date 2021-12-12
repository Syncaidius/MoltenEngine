using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public unsafe static class SilkInterop
    {
        public static void OverrideFunc(Type t, string methodName, void** vTablePtr, uint vTableIndex)
        {
            MethodInfo mi = t.GetMethod(methodName);
            if (mi != null)
            {
                void* ptrMethod = mi.MethodHandle.GetFunctionPointer().ToPointer();
                vTablePtr[vTableIndex * IntPtr.Size] = ptrMethod;
            }
            else
            {
                throw new Exception($"The method '{methodName}' was not found on type {t.FullName}");
            }
        }
    }
}

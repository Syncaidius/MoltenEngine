using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public unsafe static class SilkUtil
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

        /// <summary>Releases the specified pointer, sets it to null and returns the updated, unmanaged reference count.</summary>
        /// <typeparam name="T">The type of pointer.</typeparam>
        /// <param name="ptr">The pointer.</param>
        /// <returns>The new pointer reference count.</returns>
        public static uint ReleasePtr<T>(ref T* ptr)
            where T : unmanaged
        {
            if (ptr == null)
                return 0;

            uint r = ((IUnknown*)ptr)->Release();
            ptr = null;
            return r;
        }
    }
}

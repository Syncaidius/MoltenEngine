    using Silk.NET.Core.Attributes;
    using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    public struct IMoltenIncludeHandler
    {
        public unsafe void** LpVtbl;

        public static implicit operator IUnknown(IMoltenIncludeHandler val)
        {
            return Unsafe.As<IMoltenIncludeHandler, IUnknown>(ref val);
        }

        public unsafe IMoltenIncludeHandler(void** lpVtbl = default(void**))
        {
            this = default(IMoltenIncludeHandler);
            if (lpVtbl != null)
            {
                LpVtbl = lpVtbl;
            }

            // TODO Try using:
            //         -- void* ptrMethod = mi.MethodHandle.GetFunctionPointer().ToPointer();
            //         -- insert pointer into lpVtbl. Method attributes may be needed to allow DX API to call-back
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int QueryInterface(Guid* riid, void** ppvObject)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            IntPtr lpVtbl = (nint)(*LpVtbl);
            return ((delegate* unmanaged<IMoltenIncludeHandler*, Guid*, void**, int>)lpVtbl)(ptr, riid, ppvObject);
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int QueryInterface(Guid* riid, ref void* ppvObject)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            int result;
            fixed (void** ptr2 = &ppvObject)
            {
                IntPtr lpVtbl = (nint)(*LpVtbl);
                result = ((delegate* unmanaged<IMoltenIncludeHandler*, Guid*, void**, int>)lpVtbl)(ptr, riid, ptr2);
            }

            return result;
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int QueryInterface(ref Guid riid, void** ppvObject)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            int result;
            fixed (Guid* ptr2 = &riid)
            {
                IntPtr lpVtbl = (nint)(*LpVtbl);
                result = ((delegate* unmanaged<IMoltenIncludeHandler*, Guid*, void**, int>)lpVtbl)(ptr, ptr2, ppvObject);
            }

            return result;
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int QueryInterface(ref Guid riid, ref void* ppvObject)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            int result;
            fixed (Guid* ptr2 = &riid)
            {
                fixed (void** ptr3 = &ppvObject)
                {
                    IntPtr lpVtbl = (nint)(*LpVtbl);
                    result = ((delegate* unmanaged<IMoltenIncludeHandler*, Guid*, void**, int>)lpVtbl)(ptr, ptr2, ptr3);
                }
            }

            return result;
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly uint AddRef()
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            IntPtr intPtr = (nint)LpVtbl[1];
            return ((delegate* <IMoltenIncludeHandler*, uint>)intPtr)(ptr);
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly uint Release()
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            IntPtr intPtr = (nint)LpVtbl[2];
            return ((delegate* <IMoltenIncludeHandler*, uint>)intPtr)(ptr);
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int LoadSource(char* pFilename, IDxcBlob** ppIncludeSource)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            IntPtr intPtr = (nint)LpVtbl[3];
            return ((delegate* unmanaged<IMoltenIncludeHandler*, char*, IDxcBlob**, int>)intPtr)(ptr, pFilename, ppIncludeSource);
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int LoadSource(char* pFilename, ref IDxcBlob* ppIncludeSource)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            int result;
            fixed (IDxcBlob** ptr2 = &ppIncludeSource)
            {
                IntPtr intPtr = (nint)LpVtbl[3];
                result = ((delegate* unmanaged<IMoltenIncludeHandler*, char*, IDxcBlob**, int>)intPtr)(ptr, pFilename, ptr2);
            }

            return result;
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int LoadSource(ref char pFilename, IDxcBlob** ppIncludeSource)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            int result;
            fixed (char* ptr2 = &pFilename)
            {
                IntPtr intPtr = (nint)LpVtbl[3];
                result = ((delegate* unmanaged<IMoltenIncludeHandler*, char*, IDxcBlob**, int>)intPtr)(ptr, ptr2, ppIncludeSource);
            }

            return result;
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int LoadSource(ref char pFilename, ref IDxcBlob* ppIncludeSource)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            int result;
            fixed (char* ptr2 = &pFilename)
            {
                fixed (IDxcBlob** ptr3 = &ppIncludeSource)
                {
                    IntPtr intPtr = (nint)LpVtbl[3];
                    result = ((delegate* unmanaged<IMoltenIncludeHandler*, char*, IDxcBlob**, int>)intPtr)(ptr, ptr2, ptr3);
                }
            }

            return result;
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int LoadSource([UnmanagedType(Silk.NET.Core.Native.UnmanagedType.LPWStr)] string pFilename, IDxcBlob** ppIncludeSource)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            byte* ptr2 = (byte*)SilkMarshal.StringToPtr(pFilename, NativeStringEncoding.LPWStr);
            IntPtr intPtr = (nint)LpVtbl[3];
            int result = ((delegate* unmanaged<IMoltenIncludeHandler*, byte*, IDxcBlob**, int>)intPtr)(ptr, ptr2, ppIncludeSource);
            SilkMarshal.Free((nint)ptr2);
            return result;
        }

        //
        // Summary:
        //     To be documented.
        public unsafe readonly int LoadSource([UnmanagedType(Silk.NET.Core.Native.UnmanagedType.LPWStr)] string pFilename, ref IDxcBlob* ppIncludeSource)
        {
            IMoltenIncludeHandler* ptr = (IMoltenIncludeHandler*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            byte* ptr2 = (byte*)SilkMarshal.StringToPtr(pFilename, NativeStringEncoding.LPWStr);
            int result;
            fixed (IDxcBlob** ptr3 = &ppIncludeSource)
            {
                IntPtr intPtr = (nint)LpVtbl[3];
                result = ((delegate* unmanaged<IMoltenIncludeHandler*, byte*, IDxcBlob**, int>)intPtr)(ptr, ptr2, ptr3);
            }

            SilkMarshal.Free((nint)ptr2);
            return result;
        }
    }
}

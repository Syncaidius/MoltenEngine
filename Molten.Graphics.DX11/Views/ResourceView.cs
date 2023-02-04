using Molten.Font;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V">Underlying view type.</typeparam>
    /// <typeparam name="D">Underlying view description type.</typeparam>
    internal unsafe abstract class ResourceView<V, D>
        where V : unmanaged
        where D : unmanaged
    {
        V* _native;
        D _desc;

        internal ResourceView(DeviceDX11 device)
        {
            Device = device;
        }

        internal unsafe V* Ptr => _native;

        internal ref D Desc => ref _desc;

        internal DeviceDX11 Device { get; }

        internal void Create(ID3D11Resource* resource)
        {
            SilkUtil.ReleasePtr(ref _native);
            OnCreateView(resource, ref _desc, ref _native);
        }
        internal void SetDebugName(string debugName)
        {
            if (!string.IsNullOrWhiteSpace(debugName))
            {
                void* ptrName = (void*)SilkMarshal.StringToPtr(debugName, NativeStringEncoding.LPStr);
                ((ID3D11DeviceChild*)_native)->SetPrivateData(ref RendererDX11.WKPDID_D3DDebugObjectName, (uint)debugName.Length, ptrName);
                SilkMarshal.FreeString((nint)ptrName, NativeStringEncoding.LPStr);
            }
        }


        protected abstract void OnCreateView(ID3D11Resource* resource, ref D desc, ref V* view);

        public static implicit operator V*(ResourceView<V, D> view)
        {
            return view._native;
        }

        internal void Release()
        {
            SilkUtil.ReleasePtr(ref _native);
        }
    }
}

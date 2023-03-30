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
        GraphicsResourceFlags _requiredFlags;

        internal ResourceView(GraphicsResource resource, GraphicsResourceFlags requiredFlags)
        {
            Resource = resource;
            Device = resource.Device as DeviceDX11;
            _requiredFlags = requiredFlags;
        }

        internal unsafe V* Ptr => _native;

        internal ref D Desc => ref _desc;

        internal DeviceDX11 Device { get; }

        internal GraphicsResource Resource { get; }

        internal virtual void Create()
        {
            if (!Resource.Flags.Has(_requiredFlags))
                throw new InvalidOperationException($"Cannot create UAV for resource that does not have {_requiredFlags}");

            SilkUtil.ReleasePtr(ref _native);
            OnCreateView((ID3D11Resource*)Resource.Handle, ref _desc, ref _native);
            Device.ProcessDebugLayerMessages();
            SetDebugName($"{Resource.Name}_{GetType().Name}");
        }

        internal virtual void Create(ID3D11Resource* resource)
        {
            if (!Resource.Flags.Has(_requiredFlags))
                throw new InvalidOperationException($"Cannot create UAV for resource that does not have {_requiredFlags}");

            SilkUtil.ReleasePtr(ref _native);
            OnCreateView(resource, ref _desc, ref _native);
            SetDebugName($"{Resource.Name}_{GetType().Name}");
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

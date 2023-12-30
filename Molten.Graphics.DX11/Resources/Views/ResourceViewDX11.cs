using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V">Underlying view type.</typeparam>
    /// <typeparam name="D">Underlying view description type.</typeparam>
    internal unsafe abstract class ResourceViewDX11<V, D>
        where V : unmanaged
        where D : unmanaged
    {
        V* _native;
        D _desc;
        GraphicsResourceFlags _requiredFlags;

        internal ResourceViewDX11(ResourceHandleDX11 handle, GraphicsResourceFlags requiredFlags)
        {
            Handle = handle;
            _requiredFlags = requiredFlags;
        }

        internal unsafe V* Ptr => _native;

        internal ref D Desc => ref _desc;

        /// <summary>
        /// Gets the parent <see cref="ResourceHandleDX11"/>.
        /// </summary>
        internal ResourceHandleDX11 Handle { get; }

        internal void Create()
        {
            if (!Handle.Resource.Flags.Has(_requiredFlags))
                throw new InvalidOperationException($"Cannot create view for resource that does not have {_requiredFlags}");

            if (Handle.Ptr == null)
                throw new InvalidOperationException($"Cannot create view for resource with handle ptr of 0x0 (null).");

            NativeUtil.ReleasePtr(ref _native);

            fixed (D* ptrDesc = &_desc)
                OnCreateView((ID3D11Resource*)Handle.Ptr, ptrDesc, ref _native);

            Handle.Device.ProcessDebugLayerMessages();
            SetDebugName($"{Handle.Resource.Name}_{GetType().Name}");
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

        protected abstract void OnCreateView(ID3D11Resource* resource, D* desc, ref V* view);

        public static implicit operator V*(ResourceViewDX11<V, D> view)
        {
            return view._native;
        }

        internal void Release()
        {
            NativeUtil.ReleasePtr(ref _native);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    public abstract unsafe class ResourceHandleDX11 : GraphicsResourceHandle
    {
        internal void* InitialData;

        internal uint InitialBytes;

        public static implicit operator ID3D11Resource*(ResourceHandleDX11 handle)
        {
            return (ID3D11Resource*)handle.Ptr;
        }

        internal ResourceHandleDX11(GraphicsResource resource)
        {
            SRV = new SRView(resource);
            UAV = new UAView(resource);
        }

        public override void Dispose()
        {
            SRV.Release();
            UAV.Release();
        }

        internal SRView SRV { get; }

        internal UAView UAV { get; }
    }

    public unsafe class ResourceHandleDX11<T> : ResourceHandleDX11
        where T : unmanaged
    {
        T* _ptr;

        internal ResourceHandleDX11(GraphicsResource resource) :
            base(resource)
        { }

        public override void Dispose()
        {
            SilkUtil.ReleasePtr(ref _ptr);
        }

        public static implicit operator T*(ResourceHandleDX11<T> handle)
        {
            return handle._ptr;
        }

        public override unsafe void* Ptr => _ptr;

        internal ref T* NativePtr => ref _ptr;
    }
}

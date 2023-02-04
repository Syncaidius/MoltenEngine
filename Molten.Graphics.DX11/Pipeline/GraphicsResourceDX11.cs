using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe abstract class GraphicsResourceDX11 : GraphicsObject<ID3D11Resource>
    {
        /// <summary>Gets or sets the <see cref="ID3D11UnorderedAccessView1"/> attached to the object.</summary>
        internal UAView UAV { get; }

        /// <summary>Gets the <see cref="ID3D11ShaderResourceView1"/> attached to the object.</summary>
        internal SRView SRV { get; }

        internal GraphicsResourceDX11(DeviceDX11 device, GraphicsBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {
            SRV = new SRView(device);
            UAV = new UAView(device);
        }

        protected void SetDebugName(string debugName)
        {
            if (!string.IsNullOrWhiteSpace(debugName))
            {
                void* ptrName = (void*)SilkMarshal.StringToPtr(debugName, NativeStringEncoding.LPStr);
                NativePtr->SetPrivateData(ref RendererDX11.WKPDID_D3DDebugObjectName, (uint)debugName.Length, ptrName);
                SilkMarshal.FreeString((nint)ptrName, NativeStringEncoding.LPStr);
            }
        }

        public override void GraphicsRelease()
        {
            UAV.Release();
            SRV.Release();
        }
    }

    public unsafe abstract class ContextBindableResource<T> : GraphicsResourceDX11
        where T : unmanaged
    {
        internal ContextBindableResource(DeviceDX11 device, GraphicsBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {
        }

        /// <summary>
        /// Gets the underlying resource pointer. This should be the same address as <see cref="ContextBindable{ID3D11Resource}.NativePtr"/>
        /// </summary>
        internal abstract T* ResourcePtr { get; }

        public static implicit operator T*(ContextBindableResource<T> resource)
        {
            return resource.ResourcePtr;
        }
    }
}

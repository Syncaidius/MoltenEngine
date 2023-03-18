using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe abstract class ResourceDX11 : GraphicsResource
    {
        /// <summary>Gets or sets the <see cref="ID3D11UnorderedAccessView1"/> attached to the object.</summary>
        internal UAView UAV { get; }

        /// <summary>Gets the <see cref="ID3D11ShaderResourceView1"/> attached to the object.</summary>
        internal SRView SRV { get; }

        internal ResourceDX11(DeviceDX11 device, GraphicsBindTypeFlags bindFlags) : 
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
                ResourcePtr->SetPrivateData(ref RendererDX11.WKPDID_D3DDebugObjectName, (uint)debugName.Length, ptrName);
                SilkMarshal.FreeString((nint)ptrName, NativeStringEncoding.LPStr);
            }
        }

        public override void GraphicsRelease()
        {
            UAV.Release();
            SRV.Release();
        }

        public static implicit operator ID3D11Resource*(ResourceDX11 resource)
        {
            return resource.ResourcePtr;
        }

        /// <summary>
        /// Gets the native pointer of the current <see cref="GraphicsObject{T}"/>, as a <typeparamref name="T"/> pointer.
        /// </summary>
        internal abstract ID3D11Resource* ResourcePtr { get; }
    }

    public unsafe abstract class ResourceDX11<T> : ResourceDX11
        where T : unmanaged
    {
        internal ResourceDX11(DeviceDX11 device, GraphicsBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {
        }

        public static implicit operator T*(ResourceDX11<T> resource)
        {
            return resource.NativePtr;
        }

        /// <summary>
        /// Gets the underlying resource pointer. This should be the same address as <see cref="ContextBindable{ID3D11Resource}.NativePtr"/>
        /// </summary>
        internal abstract T* NativePtr { get; }
    }
}

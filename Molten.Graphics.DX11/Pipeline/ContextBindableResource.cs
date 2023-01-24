using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe abstract class ContextBindableResource : ContextBindable<ID3D11Resource>
    {
        /// <summary>Gets or sets the <see cref="ID3D11UnorderedAccessView1"/> attached to the object.</summary>
        internal UAView UAV { get; }

        /// <summary>Gets the <see cref="ID3D11ShaderResourceView1"/> attached to the object.</summary>
        internal SRView SRV { get; }

        internal ContextBindableResource(DeviceDX11 device, GraphicsBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {
            SRV = new SRView(device);
            UAV = new UAView(device);
        }

        public override void GraphicsRelease()
        {
            UAV.Release();
            SRV.Release();
        }
    }

    internal unsafe abstract class ContextBindableResource<T> : ContextBindableResource
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

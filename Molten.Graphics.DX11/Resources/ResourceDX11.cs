using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe abstract class ResourceDX11 : GraphicsResource
    {
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

        protected Usage GetUsageFlags()
        {
            if (Flags.Has(GraphicsResourceFlags.GpuWrite))
            {
                if (Flags.Has(GraphicsResourceFlags.CpuRead) || Flags.Has(GraphicsResourceFlags.CpuWrite))
                    return Usage.Staging;
                else
                    return Usage.Default;
            }
            else
            {
                if (Flags.Has(GraphicsResourceFlags.CpuWrite))
                    return Usage.Dynamic;
                else
                    return Usage.Immutable;
            }
        }

        protected virtual BindFlag GetBindFlags()
        {
            BindFlag result = 0;

            if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                result |= BindFlag.UnorderedAccess;

            if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                result |= BindFlag.ShaderResource;

            return result;
        }

        protected ResourceMiscFlag GetResourceFlags(bool allowMipMapGen)
        {
            ResourceMiscFlag result = 0;

            if (Flags.Has(GraphicsResourceFlags.Shared))
                result |= ResourceMiscFlag.Shared;

            if (allowMipMapGen)
                result |= ResourceMiscFlag.GenerateMips;

            return result;
        }

        protected CpuAccessFlag GetCpuFlags()
        {
            CpuAccessFlag access = CpuAccessFlag.None;

            if (Flags.Has(GraphicsResourceFlags.CpuRead))
                access |= CpuAccessFlag.Read;

            if (Flags.Has(GraphicsResourceFlags.CpuWrite))
                access |= CpuAccessFlag.Write;

            return access;
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

        internal abstract Usage UsageFlags { get; }

        /// <summary>Gets or sets the <see cref="ID3D11UnorderedAccessView1"/> attached to the object.</summary>
        internal UAView UAV { get; }

        /// <summary>Gets the <see cref="ID3D11ShaderResourceView1"/> attached to the object.</summary>
        internal SRView SRV { get; }
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

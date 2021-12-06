using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe abstract class PipeBindableResource : PipeBindable
    {
        /// <summary>Gets or sets the <see cref="ID3D11UnorderedAccessView"/> attached to the object.</summary>
        internal protected ID3D11UnorderedAccessView* UAV;

        /// <summary>Gets the <see cref="ID3D11ShaderResourceView"/> attached to the object.</summary>
        internal protected ID3D11ShaderResourceView* SRV;

        internal PipeBindableResource(DeviceDX11 device) : 
            base(device)
        {

        }

        internal override void PipelineDispose()
        {
            if(UAV != null)
            {
                UAV->Release();
                UAV = null;
            }

            if(SRV != null)
            {
                SRV->Release();
                SRV = null;
            }
        }

        #region Implicit cast operators
        public static implicit operator ID3D11UnorderedAccessView*(PipeBindableResource resource)
        {
            return resource.UAV;
        }

        public static implicit operator ID3D11ShaderResourceView*(PipeBindableResource resource)
        {
            return resource.SRV;
        }
        #endregion
    }

    internal unsafe abstract class PipeBindableResource<T> : PipeBindableResource
        where T : unmanaged
    {
        internal PipeBindableResource(DeviceDX11 device) : 
            base(device)
        {
        }

        internal abstract T* Native { get; }

        public static implicit operator ID3D11Resource*(PipeBindableResource<T> resource)
        {
            return (ID3D11Resource*)resource.Native;
        }
    }
}

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
        internal ID3D11UnorderedAccessView1* UAV;

        /// <summary>Gets the <see cref="ID3D11ShaderResourceView"/> attached to the object.</summary>
        internal ID3D11ShaderResourceView1* SRV;

        internal PipeBindableResource(PipeStageType canBindTo, PipeBindTypeFlags bindTypeFlags) : 
            base(canBindTo, bindTypeFlags)
        {

        }

        protected override bool OnValidatebind(PipeBindSlot slot)
        {
            throw new NotImplementedException();
        }

        #region Implicit cast operators
        public static implicit operator ID3D11UnorderedAccessView*(PipeBindableResource resource)
        {
            return (ID3D11UnorderedAccessView*)resource.UAV;
        }

        public static implicit operator ID3D11UnorderedAccessView1*(PipeBindableResource resource)
        {
            return resource.UAV;
        }

        public static implicit operator ID3D11ShaderResourceView*(PipeBindableResource resource)
        {
            return (ID3D11ShaderResourceView*)resource.SRV;
        }

        public static implicit operator ID3D11ShaderResourceView1*(PipeBindableResource resource)
        {
            return resource.SRV;
        }
        #endregion
    }
}

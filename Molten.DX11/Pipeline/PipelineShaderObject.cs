using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe abstract class PipelineShaderObject : PipelineObject<DeviceDX11, PipeDX11>
    {
        internal PipelineShaderObject(DeviceDX11 device) : base(device) { }

        private protected override void OnPipelineDispose()
        {
            UAV->Release();
            SRV->Release();

            UAV = null;
            SRV = null;
        }

        /// <summary>Gets or sets the <see cref="ID3D11UnorderedAccessView"/> attached to the object.</summary>
        internal ID3D11UnorderedAccessView* UAV;

        /// <summary>Gets the <see cref="ID3D11ShaderResourceView"/> attached to the object.</summary>
        internal ID3D11ShaderResourceView* SRV;
    }
}

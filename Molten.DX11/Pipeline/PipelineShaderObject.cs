using SharpDX.Direct3D11;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipelineShaderObject : PipelineObject<DeviceDX11, PipeDX11>
    {
        internal PipelineShaderObject(DeviceDX11 device) : base(device) { }

        private protected override void OnPipelineDispose()
        {
            UAV?.Dispose();
            SRV?.Dispose();

            UAV = null;
            SRV = null;
        }

        /// <summary>Gets or sets the <see cref="UnorderedAccessView"/> attached to the object.</summary>
        internal UnorderedAccessView UAV { get; set; }

        /// <summary>Gets the <see cref="ShaderResourceView"/> attached to the object.</summary>
        internal ShaderResourceView SRV { get; set; }
    }
}

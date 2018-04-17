using SharpDX.Direct3D11;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipelineShaderObject : PipelineObject
    {
        protected override void OnDispose()
        {
            UAV?.Dispose();
            SRV?.Dispose();

            base.OnDispose();
        }

        /// <summary>Gets or sets the <see cref="UnorderedAccessView"/> attached to the object.</summary>
        internal abstract UnorderedAccessView UAV { get; set; }

        /// <summary>Gets the <see cref="ShaderResourceView"/> attached to the object.</summary>
        internal abstract ShaderResourceView SRV { get; set; }
    }
}

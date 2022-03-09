using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ComputeTask : HlslShader, IComputeTask
    {
        internal RWVariable[] UAVs;
        internal CSComposition Composition;

        internal ComputeTask(Device device, string filename = null) :
            base(device, filename)
        {
            UAVs = new RWVariable[0];
            Composition = new CSComposition(this, false, ShaderType.ComputeShader);
        }

        internal override void PipelineRelease()
        {
            Composition.Dispose();
        }
    }
}

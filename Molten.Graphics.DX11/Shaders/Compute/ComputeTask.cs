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
        internal ShaderComposition<ID3D11ComputeShader> Composition;

        internal ComputeTask(DeviceDX11 device, string filename = null) :
            base(device, filename)
        {
            UAVs = new RWVariable[0];
            Composition = new ShaderComposition<ID3D11ComputeShader>(this, false);
        }

        internal override void PipelineDispose()
        {
            Composition.Dispose();
        }
    }
}

using SharpDX.Direct3D11;
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
        internal ShaderComposition<ComputeShader> Composition = new ShaderComposition<ComputeShader>(false);

        internal ComputeTask(DeviceDX11 device, string filename = null) : base(device, filename)
        {
            UAVs = new RWVariable[0];
        }

        internal override void Refresh(PipeDX11 pipe, PipelineBindSlot<DeviceDX11, PipeDX11> slot)
        {
            base.Refresh(pipe, slot);
        }
    }
}

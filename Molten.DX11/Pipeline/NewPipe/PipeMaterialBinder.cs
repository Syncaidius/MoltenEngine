using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class PipeMaterialBinder : PipeStage
    {
        ShaderVertexStage _vs;

        public PipeMaterialBinder(PipeDX11 pipe) : 
            base(pipe, PipeStageType.MaterialBinder)
        {
            _vs = new ShaderVertexStage(pipe);
        }

        internal override void Bind()
        {
            _vs.Bind();
        }
    }
}

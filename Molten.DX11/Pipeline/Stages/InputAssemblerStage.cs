using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class InputAssemblerStage : PipeStage
    {
        public InputAssemblerStage(PipeDX11 pipe, PipeStageType stageType) : 
            base(pipe, stageType)
        {

        }

        internal override void Bind()
        {
            throw new NotImplementedException();
        }
    }
}

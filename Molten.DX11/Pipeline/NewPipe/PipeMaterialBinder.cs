using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class PipeMaterialBinder : PipeStage
    {
        public PipeMaterialBinder(PipeDX11 pipe) : 
            base(pipe, PipeStageType.MaterialBinder)
        {

        }

        internal override void Bind()
        {
            
        }
    }
}

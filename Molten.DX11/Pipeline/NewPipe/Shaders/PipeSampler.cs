using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class PipeSampler : PipeBindable
    {
        public PipeSampler(PipeStageType canBindTo, PipeBindTypeFlags bindTypeFlags) : 
            base(canBindTo, bindTypeFlags)
        {

        }

        protected override void Refresh(PipeBindSlot slot, PipeDX11 pipe)
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }
    }
}

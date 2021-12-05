using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class PipeBufferSegment : PipeBindableResource<ID3D11Buffer>
    {
        internal PipeBuffer Buffer { get; set; }

        internal PipeBufferSegment(PipeStageType canBindTo, PipeBindTypeFlags bindTypeFlags) : 
            base(canBindTo, bindTypeFlags)
        {

        }

        internal override unsafe ID3D11Buffer* Native => Buffer.Native;

        protected override void OnBind(PipeBindSlot slot, PipeDX11 pipe)
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }
    }
}

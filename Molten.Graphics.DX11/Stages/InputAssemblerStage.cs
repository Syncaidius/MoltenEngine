using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class InputAssemblerStage : PipeStage
    {
        public InputAssemblerStage(DeviceContext pipe) : base(pipe)
        {
            
        }

        private void OnUnbindVB(PipeSlot<BufferSegment> slot)
        {
            Pipe.Native->IASetVertexBuffers(slot.Index, 1, null, null, null);
        }

        private void OnUnbindIB(PipeSlot<BufferSegment> slot)
        {
            Pipe.Native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
        }

        private void OnUnbindInputLayout(PipeSlot<VertexInputLayout> slot)
        {
            Pipe.Native->IASetInputLayout(null);
        }

        protected override void OnDispose()
        {
            base.OnDispose();

        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe class CommandListDX11 : GraphicsCommandList
    {
        ID3D11CommandList* _native;

        public CommandListDX11(GraphicsQueueDX11 queue, ID3D11CommandList* cmd) : 
            base(queue)
        {
            _native = cmd;
        }

        public override void Free() { }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _native);
        }
    }
}

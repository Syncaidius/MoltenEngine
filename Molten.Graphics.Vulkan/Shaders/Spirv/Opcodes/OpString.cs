using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class OpString : SpirvOpcode
    {
        public override uint ID => 7;

        protected override void Load()
        {
            base.Load();

            String.Read(Ptr + 2, WordCount - 2);
        }

        public uint ResultID => Ptr[1];

        public SpirvLiteralString String { get; } = new SpirvLiteralString();
    }
}

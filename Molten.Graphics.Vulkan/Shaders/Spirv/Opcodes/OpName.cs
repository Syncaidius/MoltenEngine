using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class OpName : SpirvOpcode
    {
        public override uint ID => 5;

        protected override void Load()
        {
            base.Load();

            Name.Read(Ptr + 2, WordCount - 2);
        }

        public uint Target => Ptr[1];

        public SpirvLiteralString Name { get; } = new SpirvLiteralString();
    }
}

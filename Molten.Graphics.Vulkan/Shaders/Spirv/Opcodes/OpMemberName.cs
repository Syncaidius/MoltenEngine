using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class OpMemberName : SpirvOpcode
    {
        public override uint ID => 6;

        protected override void Load()
        {
            base.Load();

            Name.Read(Ptr + 3, WordCount - 2);
        }
        public uint Type => Ptr[1];

        public uint Member => Ptr[2];

        public SpirvLiteralString Name { get; } = new SpirvLiteralString();
    }
}

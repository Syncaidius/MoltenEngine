using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public class OpSourceExtension : SpirvOpcode
    {
        public override uint ID => 4;

        protected unsafe override void Load()
        {
            base.Load();

            Extension.Read(Ptr + 1, WordCount - 1);
        }

        public SpirvLiteralString Extension { get; } = new SpirvLiteralString();
    }
}

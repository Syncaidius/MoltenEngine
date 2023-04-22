using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public class OpSourceContinued : SpirvOpcode
    {
        public override uint ID => 2;

        protected unsafe override void Load()
        {
            base.Load();

            Literal.Read(Ptr + 1, WordCount - 1);
        }

        public SpirvLiteralString Literal { get; } = new SpirvLiteralString();
    }
}

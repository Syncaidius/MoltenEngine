using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public unsafe class OpSource : SpirvOpcode
    {
        public override uint ID => 3;

        protected override void Load()
        {
            base.Load();

            Literal.Read(Ptr + 3, WordCount - 2);
        }

        public SourceLanguage sourceLanguage => (SourceLanguage)Ptr[1];

        public SpirvVersion Version => (SpirvVersion)Ptr[2];

        public SpirvLiteralString Literal { get; } = new SpirvLiteralString();
    }

    public enum SourceLanguage
    {
        Unknown = 0,
        ESSL = 1,
        GLSL = 2,
        OpenCL_C = 3,
        OpenCL_CPP = 4,
        HLSL = 5,
        CPP_for_OpenCL = 6,
        SYCL = 7
    }
}

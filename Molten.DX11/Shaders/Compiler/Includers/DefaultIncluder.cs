using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Shaders
{
    internal class DefaultIncluder : HlslIncluder
    {
        public DefaultIncluder(HlslCompiler compiler) : base(compiler)
        {
        }

        public override unsafe int LoadSource(char* pFilename, IDxcBlob** ppIncludeSource)
        {
            return DefaultHandler->LoadSource(pFilename, ppIncludeSource);
        }
    }
}

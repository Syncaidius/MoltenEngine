using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    public abstract class ShaderClassCompiler<R, S, CR>
        where R : RenderService
        where S : IShader
        where CR : ShaderCompileResult<S>
    {
        public abstract List<S> Parse(ShaderCompilerContext<R, S, CR> context, R renderer, string header);

        public abstract ShaderClassType ClassType { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class DxcClassCompiler<R, S> : ShaderClassCompiler<R, S, DxcCompileResult<R, S>>
        where R : RenderService
        where S : DxcFoundation
    {
        public override List<S> Parse(ShaderCompilerContext<R, S, DxcCompileResult<R, S>> context, R renderer, string header)
        {
            throw new NotImplementedException();
        }
    }
}

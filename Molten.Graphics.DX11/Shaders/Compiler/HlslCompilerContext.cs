using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class HlslCompilerContext : ShaderCompilerContext
    {

        internal ShaderCompileResult Result = new ShaderCompileResult();

        /// <summary>
        /// HLSL shader objects stored by entry-point name
        /// </summary>
        internal Dictionary<string, HlslCompileResult> HlslShaders { get; } = new Dictionary<string, HlslCompileResult>();

        internal Dictionary<string, ShaderConstantBuffer> ConstantBuffers { get; } = new Dictionary<string, ShaderConstantBuffer>();

        

        internal HlslCompiler Compiler { get; }

        internal HlslParser Parser { get; set; }

        internal HlslSource Source { get; set; }

        internal DxcArgumentBuilder Args { get; }

        internal HlslCompilerContext(HlslCompiler compiler)
        {
            Compiler = compiler;
            Args = new DxcArgumentBuilder(this);
        }
    }
}

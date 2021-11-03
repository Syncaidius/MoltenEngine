using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ShaderCompilerContext
    {
        internal ShaderCompileResult Result = new ShaderCompileResult();

        /// <summary>
        /// HLSL shader objects stored by entry-point name
        /// </summary>
        internal Dictionary<string, CompilationResult> HlslShaders = new Dictionary<string, CompilationResult>();

        internal Dictionary<string, ShaderConstantBuffer> ConstantBuffers = new Dictionary<string, ShaderConstantBuffer>();

        internal List<string> Errors = new List<string>();

        internal List<string> Messages = new List<string>();

        internal HlslCompiler Compiler;

        internal string Filename;

        internal string Source;
    }
}

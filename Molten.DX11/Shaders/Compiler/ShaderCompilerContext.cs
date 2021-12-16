using Silk.NET.Direct3D.Compilers;
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
        internal Dictionary<string, HlslCompileResult> HlslShaders { get; } = new Dictionary<string, HlslCompileResult>();

        internal Dictionary<string, ShaderConstantBuffer> ConstantBuffers { get; } = new Dictionary<string, ShaderConstantBuffer>();

        internal List<string> Errors { get; } = new List<string>();

        internal List<string> Messages { get; } = new List<string>();

        internal HlslCompiler Compiler { get; set; }

        internal string Filename { get; set; }

        internal string Source { get; set; }
    }
}

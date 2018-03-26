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

        /// <summary>
        /// Rasterizer states, stored by their .ToString() result.
        /// </summary>
        internal Dictionary<string, GraphicsRasterizerState> RasterStates = new Dictionary<string, GraphicsRasterizerState>();

        /// <summary>
        /// Blend states, stored by their .ToString() result.
        /// </summary>
        internal Dictionary<string, GraphicsBlendState> BlendStates = new Dictionary<string, GraphicsBlendState>();

        /// <summary>
        /// Depth states, stored by their .ToString() result.
        /// </summary>
        internal Dictionary<string, GraphicsDepthState> DepthStates = new Dictionary<string, GraphicsDepthState>();

        /// <summary>
        /// Sampler states, stored by their .ToString() result.
        /// </summary>
        internal Dictionary<string, ShaderSampler> Samplers = new Dictionary<string, ShaderSampler>();

        internal ShaderCompilerContext(HlslCompiler compiler)
        {
            Compiler = compiler;
        }

        internal HlslCompiler Compiler { get; private set; }

        internal string Filename;

        internal string Source;

        internal string Header;
    }
}

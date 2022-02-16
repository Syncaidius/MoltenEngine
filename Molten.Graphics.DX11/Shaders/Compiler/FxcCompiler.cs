using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    internal unsafe class FxcCompiler : ShaderCompiler<RendererDX11, HlslFoundation, FxcCompileResult>
    {
        internal D3DCompiler Compiler { get; }

        /// <summary>
        /// Creates a new instance of <see cref="FxcCompiler"/>.
        /// </summary>
        /// <param name="renderer">The renderer which owns the compiler.</param>
        /// <param name="log"></param>
        /// <param name="includePath">The default path for engine/game HLSL include files.</param>
        /// <param name="includeAssembly"></param>
        internal FxcCompiler(RendererDX11 renderer, Logger log, string includePath, Assembly includeAssembly) :
            base(renderer, includePath, includeAssembly)
        {
            Compiler = D3DCompiler.GetApi();

            AddClassCompiler<MaterialCompiler>();
            AddClassCompiler<ComputeCompiler>();
        }

        protected override void OnDispose()
        {
            Compiler.Dispose();
        }

        /// <summary>Compiles HLSL source code and outputs the result. Returns true if successful, or false if there were errors.</summary>
        /// <param name="entryPoint"></param>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool CompileSource(string entryPoint, ShaderType type, 
            ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context, out FxcCompileResult result)
        {
            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.Shaders.TryGetValue(entryPoint, out result))
            {
                /*context.Args.SetEntryPoint(entryPoint);
                context.Args.SetShaderProfile(ShaderModel.Model5_0, type);

                string argString = context.Args.ToString();
                uint argCount = context.Args.Count;
                char** ptrArgString = context.Args.GetArgsPtr();

                Guid dxcResultGuid = IDxcResult.Guid;
                void* dxcResult;
                Buffer srcBuffer = context.Source.BuildSource(context.Compiler);

                Native->Compile(&srcBuffer, ptrArgString, argCount, null, &dxcResultGuid, &dxcResult);
                result = new ShaderC(context, (IDxcResult*)dxcResult);

                SilkMarshal.Free((nint)ptrArgString);

                if (context.HasErrors)
                    return false;

                context.Shaders.Add(entryPoint, result);*/
            }

            return true;
        }
    }
}

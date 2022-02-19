using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System.Reflection;

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

        protected override List<Type> GetNodeParserList()
        {
            return ReflectionHelper.FindTypeInParentAssembly<FxcNodeParser>();
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
                string shaderProfile = ShaderModel.Model5_0.ToProfile(type, ShaderLanguage.Hlsl);

                byte* pSourceName = (byte*)SilkMarshal.StringToPtr(context.Source.Filename, NativeStringEncoding.LPStr);
                byte* pEntryPoint = (byte*)SilkMarshal.StringToPtr(entryPoint, NativeStringEncoding.LPStr);
                byte* pTarget = (byte*)SilkMarshal.StringToPtr(shaderProfile, NativeStringEncoding.LPStr);
                void* pSrc = (void*)SilkMarshal.StringToPtr(context.Source.SourceCode, NativeStringEncoding.Ansi);
                FxcCompileFlags compileFlags = context.Flags.Translate();

                ID3D10Blob* pByteCode = null;
                ID3D10Blob* pErrors = null;

                HResult r = Compiler.Compile(pSrc, context.Source.NumBytes, pSourceName, null, null, pEntryPoint, pTarget, (uint)compileFlags, 0, &pByteCode, &pErrors);

                result = new FxcCompileResult(context, Compiler, pByteCode, pErrors);

                if (context.HasErrors)
                    return false;

                context.Shaders.Add(entryPoint, result);

                SilkMarshal.Free((nint)pSourceName);
                SilkMarshal.Free((nint)pEntryPoint);
                SilkMarshal.Free((nint)pTarget);
            }

            return true;
        }
    }
}

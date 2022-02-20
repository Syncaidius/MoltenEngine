using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System.Reflection;

namespace Molten.Graphics
{
    internal unsafe class FxcCompiler : ShaderCompiler<RendererDX11, HlslFoundation>
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
        internal bool CompileSource(string entryPoint, ShaderType type, 
            ShaderCompilerContext<RendererDX11, HlslFoundation> context, out FxcCompileResult result)
        {
            IShaderClassResult classResult = null;

            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.Shaders.TryGetValue(entryPoint, out classResult))
            {
                string shaderProfile = ShaderModel.Model5_0.ToProfile(type, ShaderLanguage.Hlsl);

                byte* pSourceName = (byte*)SilkMarshal.StringToPtr(context.Source.Filename, NativeStringEncoding.LPStr);
                byte* pEntryPoint = (byte*)SilkMarshal.StringToPtr(entryPoint, NativeStringEncoding.LPStr);
                byte* pTarget = (byte*)SilkMarshal.StringToPtr(shaderProfile, NativeStringEncoding.LPStr);
                void* pSrc = (void*)SilkMarshal.StringToPtr(context.Source.SourceCode, NativeStringEncoding.LPStr);
                FxcCompileFlags compileFlags = context.Flags.Translate();

                ID3D10Blob* pByteCode = null;
                ID3D10Blob* pErrors = null;
                ID3D10Blob* pProcessedSrc = null;

                uint numBytes = (uint)SilkMarshal.GetMaxSizeOf(context.Source.SourceCode, NativeStringEncoding.LPStr);

                // Preprocess and check for errors
                HResult r = Compiler.Preprocess(pSrc, numBytes, pSourceName, null, null, &pProcessedSrc, &pErrors);
                ParseErrors(context, pErrors);

                // Compile source and check for errors
                if (!context.HasErrors)
                {
                    void* postProcessedSrc = pProcessedSrc->GetBufferPointer();
                    nuint postProcessedSize = pProcessedSrc->GetBufferSize();

                    r = Compiler.Compile(postProcessedSrc, postProcessedSize, pSourceName, null, null, pEntryPoint, pTarget, (uint)compileFlags, 0, &pByteCode, &pErrors);
                    ParseErrors(context, pErrors);
                }

                //Store shader result
                if (!context.HasErrors)
                {
                    classResult = new FxcCompileResult(context, Compiler, pByteCode);
                    context.Shaders.Add(entryPoint, classResult);
                }

                SilkUtil.ReleasePtr(ref pProcessedSrc);
                SilkUtil.ReleasePtr(ref pErrors);
                SilkMarshal.Free((nint)pSrc);
                SilkMarshal.Free((nint)pSourceName);
                SilkMarshal.Free((nint)pEntryPoint);
                SilkMarshal.Free((nint)pTarget);
            }

            result = classResult as FxcCompileResult;
            return !context.HasErrors;
        }

        private void ParseErrors(ShaderCompilerContext<RendererDX11, HlslFoundation> context, ID3D10Blob* errors)
        {
            if (errors == null)
                return;

            void* ptrErrors = errors->GetBufferPointer();
            nuint numBytes = errors->GetBufferSize();
            string strErrors = SilkMarshal.PtrToString((nint)ptrErrors, NativeStringEncoding.UTF8);
            string[] errorList = strErrors.Split('\r', '\n');

            for (int i = 0; i < errorList.Length; i++)
                context.AddError(errorList[i]);
        }
    }
}

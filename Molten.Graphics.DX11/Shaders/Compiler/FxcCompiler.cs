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
        // For reference or help see the following:
        // See: https://github.com/microsoft/DirectXShaderCompiler/blob/master/include/dxc/dxcapi.h
        // See: https://posts.tanki.ninja/2019/07/11/Using-DXC-In-Practice/
        // See: https://simoncoenen.com/blog/programming/graphics/DxcCompiling

        static readonly Guid CLSID_DxcLibrary= new Guid(0x6245d6af, 0x66e0, 0x48fd, 
            new byte[] {0x80, 0xb4, 0x4d, 0x27, 0x17, 0x96, 0x74, 0x8c});

        static readonly Guid CLSID_DxcUtils = CLSID_DxcLibrary;

        static readonly Guid CLSID_DxcCompilerArgs = new Guid(0x3e56ae82, 0x224d, 0x470f,
            new byte[] { 0xa1, 0xa1, 0xfe, 0x30, 0x16, 0xee, 0x9f, 0x9d });

        static readonly Guid CLSID_DxcCompiler = new Guid(0x73e22d93U, (ushort)0xe6ceU, (ushort)0x47f3U, 
            0xb5, 0xbf, 0xf0, 0x66, 0x4f, 0x39, 0xc1, 0xb0 );

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
            _utils = CreateDxcInstance<IDxcUtils>(CLSID_DxcUtils, IDxcUtils.Guid);
            _compiler = CreateDxcInstance<IDxcCompiler3>(CLSID_DxcCompiler, IDxcCompiler3.Guid);

            // Detect and instantiate node parsers
            IEnumerable<Type> parserTypes = ReflectionHelper.FindTypeInParentAssembly<ShaderNodeParser<RendererDX11, HlslFoundation, FxcCompileResult>>();
            foreach (Type t in parserTypes)
            {
                BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                ShaderNodeParser<RendererDX11, HlslFoundation, FxcCompileResult> parser = 
                    Activator.CreateInstance(t, bFlags, null, null, null) as ShaderNodeParser<RendererDX11, HlslFoundation, FxcCompileResult>;
                
                foreach (string nodeName in parser.SupportedNodes)
                    NodeParsers.Add(nodeName, parser);
            }

            AddClassCompiler<MaterialCompiler>();
            AddClassCompiler<ComputeCompiler>();
        }

        protected override void OnDispose()
        {

         
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

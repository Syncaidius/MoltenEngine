using Molten.Collections;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    internal unsafe class HlslCompiler : EngineObject
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


        Dictionary<string, HlslParser> _shaderParsers;
       
        IDxcCompiler3* _compiler;
        IDxcUtils* _utils;

        /// <summary>
        /// Creates a new instance of <see cref="HlslCompiler"/>.
        /// </summary>
        /// <param name="renderer">The renderer which owns the compiler.</param>
        /// <param name="log"></param>
        /// <param name="includePath">The default path for engine/game HLSL include files.</param>
        /// <param name="includeAssembly"></param>
        internal HlslCompiler(RendererDX11 renderer, Logger log, string includePath, Assembly includeAssembly)
        {
            _shaderParsers = new Dictionary<string, HlslParser>();

            Dxc = DXC.GetApi();
            _utils = CreateDxcInstance<IDxcUtils>(CLSID_DxcUtils, IDxcUtils.Guid);
            _compiler = CreateDxcInstance<IDxcCompiler3>(CLSID_DxcCompiler, IDxcCompiler3.Guid);

            // Detect and instantiate node parsers
            IEnumerable<Type> parserTypes = ReflectionHelper.FindTypeInParentAssembly<ShaderNodeParser>();
            foreach (Type t in parserTypes)
            {
                ShaderNodeParser parser = Activator.CreateInstance(t, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null) as ShaderNodeParser;
                foreach (string nodeName in parser.SupportedNodes)
                    NodeParsers.Add(nodeName, parser);
            }

            AddSubCompiler<MaterialParser>("material");
            AddSubCompiler<ComputeParser>("compute");
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _compiler);
            SilkUtil.ReleasePtr(ref _utils);
            Dxc.Dispose();
        }

        private T* CreateDxcInstance<T>(Guid clsid, Guid iid) where T : unmanaged
        {
            void* ppv = null;
            HResult result = Dxc.CreateInstance(&clsid, &iid, ref ppv);
            return (T*)ppv;
        }

        private void AddSubCompiler<T>(string nodeName) where T : HlslParser
        {
            Type t = typeof(T);
            BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            HlslParser sub = Activator.CreateInstance(t, bindFlags,  null, null, null) as HlslParser;
            _shaderParsers.Add(nodeName, sub);
        }

        /// <summary>Compiles HLSL source code and outputs the result. Returns true if successful, or false if there were errors.</summary>
        /// <param name="log"></param>
        /// <param name="entryPoint"></param>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="filename"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal bool CompileHlsl(string entryPoint, ShaderType type, HlslCompilerContext context, out HlslCompileResult result)
        {
            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.HlslShaders.TryGetValue(entryPoint, out result))
            {
                context.Args.SetEntryPoint(entryPoint);
                context.Args.SetShaderProfile(ShaderModel.Model5_0, type);

                string argString = context.Args.ToString();
                uint argCount = context.Args.Count;
                char** ptrArgString = context.Args.GetArgsPtr();

                Guid dxcResultGuid = IDxcResult.Guid;
                void* dxcResult;
                Buffer srcBuffer = context.Source.BuildSource(context.Compiler);

                Native->Compile(&srcBuffer, ptrArgString, argCount, null, &dxcResultGuid, &dxcResult);
                result = new HlslCompileResult(context, (IDxcResult*)dxcResult);

                SilkMarshal.Free((nint)ptrArgString);

                if (context.HasErrors)
                    return false;

                context.HlslShaders.Add(entryPoint, result);
            }

            return true;
        }

        internal DXC Dxc { get; }

        internal Device Device => _renderer.Device;

        internal RendererDX11 Renderer => _renderer;

        internal IDxcCompiler3* Native => _compiler;

        internal IDxcUtils* Utils => _utils;
    }
}

using System.Reflection;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    public unsafe class DxcCompiler : ShaderCompiler
    {
        // For reference or help see the following:
        // See: https://github.com/microsoft/DirectXShaderCompiler/blob/master/include/dxc/dxcapi.h
        // See: https://posts.tanki.ninja/2019/07/11/Using-DXC-In-Practice/
        // See: https://simoncoenen.com/blog/programming/graphics/DxcCompiling

        internal static readonly Guid CLSID_DxcLibrary= new Guid(0x6245d6af, 0x66e0, 0x48fd, 
            new byte[] {0x80, 0xb4, 0x4d, 0x27, 0x17, 0x96, 0x74, 0x8c});

        internal static readonly Guid CLSID_DxcUtils = CLSID_DxcLibrary;

        internal static readonly Guid CLSID_DxcCompilerArgs = new Guid(0x3e56ae82, 0x224d, 0x470f,
            new byte[] { 0xa1, 0xa1, 0xfe, 0x30, 0x16, 0xee, 0x9f, 0x9d });

        internal static readonly Guid CLSID_DxcCompiler = new Guid(0x73e22d93U, (ushort)0xe6ceU, (ushort)0x47f3U, 
            0xb5, 0xbf, 0xf0, 0x66, 0x4f, 0x39, 0xc1, 0xb0 );

        internal static readonly Guid CLSID_DxcContainerReflection = new Guid(0xb9f54489, 0x55b8, 0x400c,
            0xba, 0x3a, 0x16, 0x75, 0xe4, 0x72, 0x8b, 0x91);
       
        IDxcCompiler3* _compiler;
        IDxcUtils* _utils;
        Dictionary<ShaderSource, DxcSourceBlob> _sourceBlobs;

        /// <summary>
        /// Creates a new instance of <see cref="DxcCompiler"/>.
        /// </summary>
        /// <param name="renderer">The renderer which owns the compiler.</param>
        /// <param name="includePath">The default path for engine/game HLSL include files.</param>
        /// <param name="includeAssembly"></param>
        public DxcCompiler(RenderService renderer, string includePath, Assembly includeAssembly) : 
            base(renderer, includePath, includeAssembly)
        {
            _sourceBlobs = new Dictionary<ShaderSource, DxcSourceBlob>();

            Dxc = DXC.GetApi();
            _utils = CreateDxcInstance<IDxcUtils>(CLSID_DxcUtils, IDxcUtils.Guid);
            _compiler = CreateDxcInstance<IDxcCompiler3>(CLSID_DxcCompiler, IDxcCompiler3.Guid);
        }

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _compiler);
            SilkUtil.ReleasePtr(ref _utils);
            Dxc.Dispose();
        }

        private T* CreateDxcInstance<T>(Guid clsid, Guid iid) 
            where T : unmanaged
        {
            void* ppv = null;
            HResult result = Dxc.CreateInstance(&clsid, &iid, ref ppv);
            return (T*)ppv;
        }

        protected override unsafe ShaderReflection BuildReflection(ShaderCompilerContext context, void* ptrData)
        {
            IDxcResult* dxcResult = (IDxcResult*)ptrData;
            IDxcContainerReflection* containerReflection = LoadReflection(context, dxcResult);

            ShaderReflection r = new ShaderReflection();

            // TODO populate reflection

            return r;
        }

        /// <summary>Compiles HLSL source code and outputs the result. Returns true if successful, or false if there were errors.</summary>
        /// <param name="entryPoint"></param>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// 
        public override bool CompileSource(string entryPoint, ShaderType type, 
            ShaderCompilerContext context, out ShaderCodeResult result)
        {
            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.Shaders.TryGetValue(entryPoint, out result))
            {
                DxcArgumentBuilder args = new DxcArgumentBuilder(context);
                args.SetEntryPoint(entryPoint);
                args.SetShaderProfile(ShaderModel.Model5_0, type);

                string argString = args.ToString();
                uint argCount = args.Count;
                char** ptrArgString = args.GetArgsPtr();

                Guid dxcResultGuid = IDxcResult.Guid;
                void* ptrResult;

                DxcSourceBlob srcBlob = BuildSource(context.Source);

                Native->Compile(in srcBlob.BlobBuffer, ptrArgString, argCount, null, &dxcResultGuid, &ptrResult);

                IDxcResult* dxcResult = (IDxcResult*)ptrResult;
                IDxcBlob* byteCode = null;
                IDxcBlob* pdbData = null;
                string pdbPath = "";
                List<OutKind> _availableOutputs;

                _availableOutputs = new List<OutKind>();
                dxcResult->GetResult(&byteCode);

                uint numOutputs = dxcResult->GetNumOutputs();
                context.AddDebug($"{numOutputs} DXC outputs found: ");

                LoadPdbData(context, dxcResult, ref pdbData, ref pdbPath);
                LoadErrors(context, dxcResult);
                OutKind[] outTypes = Enum.GetValues<OutKind>();

                // List all available outputs
                foreach (OutKind kind in outTypes)
                {
                    if (kind == OutKind.None)
                        continue;

                    if(dxcResult->HasOutput(kind) > 0)
                        context.AddDebug($"\t{kind}");
                }

                SilkMarshal.Free((nint)ptrArgString);
                SilkUtil.ReleasePtr(ref pdbData);

                if (context.HasErrors)
                    return false;

                ShaderReflection reflection = BuildReflection(context, dxcResult);
                result = new ShaderCodeResult(reflection, byteCode, byteCode->GetBufferSize());
                context.Shaders.Add(entryPoint, result);
            }

            return true;
        }

        private void LoadPdbData(ShaderCompilerContext context, IDxcResult* dxcResult, ref IDxcBlob* data, ref string pdbPath)
        {
            IDxcBlobUtf16* pPdbPath = null;

            if (GetDxcOutput(context, OutKind.Pdb, dxcResult, ref data, &pPdbPath))
            {
                pdbPath = pPdbPath->GetStringPointerS();
                nuint dataSize = data->GetBufferSize();
                context.AddDebug($"\t Loaded DXC PDB data -- Bytes: {dataSize} -- Path: {pdbPath}");

                SilkUtil.ReleasePtr(ref pPdbPath);
            }
        }

        private IDxcContainerReflection* LoadReflection(ShaderCompilerContext context, IDxcResult* dxcResult)
        {
            IDxcBlob* outData = null;
            DxcBuffer* reflectionBuffer = null;
            void* pReflection = null;
            Guid guidReflection = CLSID_DxcContainerReflection;

            if (GetDxcOutput(context, OutKind.Reflection, dxcResult, ref outData))
            {
                _utils->CreateReflection(reflectionBuffer, ref guidReflection, ref pReflection);

                nuint dataSize = outData->GetBufferSize();
                context.AddDebug($"\t Loaded DXC container reflection data -- Bytes: {dataSize}");
            }

            return (IDxcContainerReflection*)pReflection;
        }

        private void LoadErrors(ShaderCompilerContext context, IDxcResult* dxcResult)
        {
            if (dxcResult->HasOutput(OutKind.Errors) == 0)
                return;

            IDxcBlobEncoding* pErrorBlob = null;
            dxcResult->GetErrorBuffer(&pErrorBlob);

            void* ptrErrors = pErrorBlob->GetBufferPointer();
            nuint numBytes = pErrorBlob->GetBufferSize();
            string strErrors = SilkMarshal.PtrToString((nint)ptrErrors, NativeStringEncoding.UTF8);

            string[] errors = strErrors.Split('\r', '\n');
            for (int i = 0; i < errors.Length; i++)
                context.AddError(errors[i]);

            SilkUtil.ReleasePtr(ref pErrorBlob);
        }

        /// <summary>
        /// Retrieves the debug PDB data from a shader compilation result (<see cref="IDxcResult"/>).
        /// </summary>
        /// <param name="result"></param>
        /// <param name="outData"></param>
        /// <param name="outPath"></param>
        private bool GetDxcOutput(ShaderCompilerContext context, OutKind outputType, IDxcResult* dxcResult,
            ref IDxcBlob* outData, IDxcBlobUtf16** outPath = null)
        {
            if (dxcResult->HasOutput(outputType) == 0)
            {
                context.AddError($"Unable to retrieve '{outputType}' data: Not found");
                return false;
            }

            void* pData = null;
            IDxcBlobUtf16* pDataPath = null;
            Guid iid = IDxcBlob.Guid;
            dxcResult->GetOutput(outputType, &iid, &pData, outPath);
            outData = (IDxcBlob*)pData;
            return true;
        }

        /// <summary>
        /// (Re)builds the HLSL source code in the current <see cref="HlslSource"/> instance. 
        /// This generates a (new) <see cref="DxcBuffer"/> object.
        /// </summary>
        /// <param name="compiler"></param>
        /// <returns></returns>
        internal DxcSourceBlob BuildSource(ShaderSource source)
        {
            if(!_sourceBlobs.TryGetValue(source, out DxcSourceBlob blob))
            {
                blob = new DxcSourceBlob();
                void* ptrSource = (void*)SilkMarshal.StringToPtr(source.SourceCode, NativeStringEncoding.UTF8);
                uint numBytes = (uint)SilkMarshal.GetMaxSizeOf(source.SourceCode, NativeStringEncoding.UTF8);

                _utils->CreateBlob(ptrSource, numBytes, DXC.CPUtf16, ref blob.Ptr);

                blob.BlobBuffer = new DxcBuffer()
                {
                    Ptr = blob.Ptr->GetBufferPointer(),
                    Size = blob.Ptr->GetBufferSize(),
                    Encoding = 0
                };
            }

            return blob;
        }

        public override ShaderIOStructure BuildIO(ShaderCodeResult result, ShaderIOStructureType type)
        {
            throw new NotImplementedException();
        }

        public override bool BuildStructure(ShaderCompilerContext context, HlslShader shader, ShaderCodeResult result, ShaderComposition composition)
        {
            throw new NotImplementedException();
        }

        public override unsafe void* BuildShader(HlslElement element, ShaderType type, void* byteCode)
        {
            throw new NotImplementedException();
        }

        internal DXC Dxc { get; }

        internal IDxcCompiler3* Native => _compiler;

        internal IDxcUtils* Utils => _utils;
    }
}

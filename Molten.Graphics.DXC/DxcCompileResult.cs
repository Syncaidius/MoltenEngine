using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;

using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    /// <summary>
    /// Provides an abstraction over the top of <see cref="IDxcResult"/> for retrieving various output elements.
    /// </summary>
    /// <typeparam name="R">Render service type</typeparam>
    /// <typeparam name="S">DXC shader type.</typeparam>
    public unsafe class DxcCompileResult<R, S> : ShaderClassResult
        where R : RenderService
        where S : DxcFoundation
    {
        
        internal IDxcResult* Result;
        IDxcContainerReflection* _containerReflection;
        DxcReflection _reflection;
        IDxcBlob* _byteCode;
        IDxcUtils* _utils;
        IDxcBlob* _pdbData;
        List<OutKind> _availableOutputs;

        internal IReadOnlyCollection<OutKind> AvailableOutputs { get; }

        internal IDxcBlob* PdbData => _pdbData;

        internal string PdbPath { get; private set; }

        internal IDxcBlob* ByteCode => _byteCode;

        internal DxcCompileResult(ShaderCompilerContext<R,S> context, IDxcUtils* utils, IDxcResult* result)
        {
            _utils = utils;
            _availableOutputs = new List<OutKind>();
            AvailableOutputs = _availableOutputs.AsReadOnly();
            Result = result;

            LoadByteCode();

            uint numOutputs = Result->GetNumOutputs();
            context.AddDebug($"{numOutputs} DXC outputs found: ");

            OutKind[] outTypes = Enum.GetValues<OutKind>();
            foreach(OutKind kind in outTypes)
            {
                if (kind == OutKind.None)
                    continue;

                bool hasOutput = Result->HasOutput(kind) > 0;
                if (!hasOutput)
                    continue;

                context.AddDebug($"\t{kind}");
                _availableOutputs.Add(kind);

                switch (kind)
                {
                    default:
                    case OutKind.None:
                        context.AddWarning($"\t Unsupported output-kind in DXC result: {kind}");
                        break;

                    case OutKind.Pdb: LoadPdbData(context); break;
                    case OutKind.Reflection: LoadReflection(context); break;

                    case OutKind.Errors: LoadErrors(context); break;
                }
            }
        }
 
        protected override void OnDispose()
        {
            _reflection.Dispose();
            SilkUtil.ReleasePtr(ref _pdbData);
            SilkUtil.ReleasePtr(ref _byteCode);
        }

        /// <summary>
        /// Retrieves the debug PDB data from a shader compilation result (<see cref="IDxcResult"/>).
        /// </summary>
        /// <param name="result"></param>
        /// <param name="outData"></param>
        /// <param name="outPath"></param>
        private void GetDxcOutput(OutKind outputType,
            ref IDxcBlob* outData, IDxcBlobUtf16** outPath = null)
        {
            void* pData = null;
            IDxcBlobUtf16* pDataPath = null;
            Guid iid = IDxcBlob.Guid;
            Result->GetOutput(outputType, &iid, &pData, outPath);
            outData = (IDxcBlob*)pData;
        }

        private void LoadPdbData(ShaderCompilerContext<R, S> context)
        {
            IDxcBlobUtf16* pPdbPath = null;
            GetDxcOutput(OutKind.Pdb, ref _pdbData, &pPdbPath);

            PdbPath = pPdbPath->GetStringPointerS();
            nuint dataSize = _pdbData->GetBufferSize();
            context.AddDebug($"\t Loaded DXC PDB data -- Bytes: {dataSize} -- Path: {PdbPath}");

            SilkUtil.ReleasePtr(ref pPdbPath);
        }

        private void LoadReflection(ShaderCompilerContext<R, S> context)
        {
            IDxcBlob* outData = null;
            DxcBuffer* reflectionBuffer = null;
            void* pReflection = null;
            Guid guidReflection = DxcCompiler<R, S>.CLSID_DxcContainerReflection;

            GetDxcOutput(OutKind.Reflection, ref outData);
            _utils->CreateReflection(reflectionBuffer, ref guidReflection, ref pReflection);

            nuint dataSize = outData->GetBufferSize();
            context.AddDebug($"\t Loaded DXC container reflection data -- Bytes: {dataSize}");
        }

        public T GetReflection<T>() where T : DxcReflection, new()
        {
            if(_reflection == null)
            {
                _reflection = new T();
                _reflection.SetDxcReflection(_containerReflection);
            }

            return _reflection as T;
        }

        private void LoadByteCode()
        {
            Result->GetResult(ref _byteCode);
        }

        private void LoadErrors(ShaderCompilerContext<R, S> context)
        {
            IDxcBlobEncoding* pErrorBlob = null;
            Result->GetErrorBuffer(&pErrorBlob);

            void* ptrErrors = pErrorBlob->GetBufferPointer();
            nuint numBytes = pErrorBlob->GetBufferSize();
            string strErrors = SilkMarshal.PtrToString((nint)ptrErrors, NativeStringEncoding.UTF8);

            string[] errors = strErrors.Split('\r', '\n');
            for (int i = 0; i < errors.Length; i++)
                context.AddError(errors[i]);

            SilkUtil.ReleasePtr(ref pErrorBlob);
        }
    }
}

using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    /// <summary>
    /// Provides an abstraction over the top of <see cref="IDxcResult"/> for retrieving various output elements.
    /// </summary>
    internal unsafe class HlslCompileResult : EngineObject
    {
        
        internal IDxcResult* Result;
        IDxcBlob* _byteCode;
        IDxcUtils* _utils;
        IDxcBlob* _pdbData;
        List<OutKind> _availableOutputs;

        internal IReadOnlyCollection<OutKind> AvailableOutputs { get; }

        internal HlslReflection Reflection { get; private set; }

        internal IDxcBlob* PdbData => _pdbData;

        internal string PdbPath { get; private set; }

        internal IDxcBlob* ByteCode => _byteCode;

        internal HlslCompileResult(HlslCompilerContext context, IDxcResult* result)
        {
            _utils = context.Compiler.Utils;
            _availableOutputs = new List<OutKind>();
            AvailableOutputs = _availableOutputs.AsReadOnly();
            Result = result;

            LoadByteCode();

            uint numOutputs = Result->GetNumOutputs();
            context.AddDebug($"{numOutputs} DXC outputs found: ");

            OutKind[] outTypes = Enum.GetValues<OutKind>();
            foreach(OutKind kind in outTypes)
            {
                if (kind == OutKind.OutNone)
                    continue;

                bool hasOutput = Result->HasOutput(kind) > 0;
                if (!hasOutput)
                    continue;

                context.AddDebug($"\t{kind}");
                _availableOutputs.Add(kind);

                switch (kind)
                {
                    default:
                    case OutKind.OutNone:
                        context.AddWarning($"\t Unsupported output-kind in DXC result: {kind}");
                        break;

                    case OutKind.OutPdb: LoadPdbData(context); break;
                    case OutKind.OutReflection: LoadReflection(context); break;

                    case OutKind.OutErrors: LoadErrors(context); break;
                }
            }
        }
 
        protected override void OnDispose()
        {
            Reflection.Dispose();
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

        private void LoadPdbData(HlslCompilerContext context)
        {
            IDxcBlobUtf16* pPdbPath = null;
            GetDxcOutput(OutKind.OutPdb, ref _pdbData, &pPdbPath);

            PdbPath = pPdbPath->GetStringPointerS();
            nuint dataSize = _pdbData->GetBufferSize();
            context.AddDebug($"\t Loaded DXC PDB data -- Bytes: {dataSize} -- Path: {PdbPath}");

            SilkUtil.ReleasePtr(ref pPdbPath);
        }

        private void LoadReflection(HlslCompilerContext context)
        {
            IDxcBlob* outData = null;
            DxcBuffer* reflectionBuffer = null;
            Guid iid = ID3D11ShaderReflection.Guid;
            void* pReflection = null;

            GetDxcOutput(OutKind.OutReflection, ref outData);
            _utils->CreateReflection(reflectionBuffer, ref iid, ref pReflection);
            Reflection = new HlslReflection((ID3D11ShaderReflection*)pReflection);

            nuint dataSize = outData->GetBufferSize();
            context.AddDebug($"\t Loaded DXC Reflection data -- Bytes: {dataSize}");
        }

        private void LoadByteCode()
        {
            Result->GetResult(ref _byteCode);
        }

        private void LoadErrors(HlslCompilerContext context)
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

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
        Logger _log;

        IDxcBlob* _pdbData;

        internal HlslReflection Reflection { get; private set; }

        internal IDxcBlob* PdbData => _pdbData;

        internal string PdbPath { get; private set; }

        internal IDxcBlob* ByteCode => _byteCode;

        internal HlslCompileResult(IDxcUtils* utils, IDxcResult* result, Logger log)
        {
            _utils = utils;
            _log = log;
            Result = result;

            LoadByteCode();

            uint numOutputs = Result->GetNumOutputs();
            log.WriteDebugLine($"{numOutputs} DXC outputs found: ");

            for (uint i = 0; i < numOutputs; i++)
            {
                OutKind o = Result->GetOutputByIndex(i);

                switch (o)
                {
                    default:
                    case OutKind.OutNone:
                        _log.WriteWarning($"\t Unsupported output-kind in DXC result: {o}");
                        break;

                    case OutKind.OutPdb: LoadPdbData(); break;
                    case OutKind.OutReflection: LoadReflection(); break;

                    case OutKind.OutErrors: WriteErrors(); break;
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

        private void LoadPdbData()
        {
            IDxcBlobUtf16* pPdbPath = null;
            GetDxcOutput(OutKind.OutPdb, ref _pdbData, &pPdbPath);

            PdbPath = pPdbPath->GetStringPointerS();
            nuint dataSize = _pdbData->GetBufferSize();
            _log.WriteDebugLine($"\t Loaded DXC PDB data -- Bytes: {dataSize} -- Path: {PdbPath}");

            SilkUtil.ReleasePtr(ref pPdbPath);
        }

        private void LoadReflection()
        {
            IDxcBlob* outData = null;
            DxcBuffer* reflectionBuffer = null;
            Guid iid = ID3D11ShaderReflection.Guid;
            void* pReflection = null;

            GetDxcOutput(OutKind.OutReflection, ref outData);
            _utils->CreateReflection(reflectionBuffer, ref iid, ref pReflection);
            Reflection = new HlslReflection((ID3D11ShaderReflection*)pReflection);

            nuint dataSize = outData->GetBufferSize();
            _log.WriteDebugLine($"\t Loaded DXC Reflection data -- Bytes: {dataSize}");
        }

        private void LoadByteCode()
        {
            Result->GetResult(ref _byteCode);
        }

        private void WriteErrors()
        {
            IDxcBlobEncoding* pErrorBlob = null;
            Result->GetErrorBuffer(&pErrorBlob);

            void* ptrErrors = pErrorBlob->GetBufferPointer();
            nuint numBytes = pErrorBlob->GetBufferSize();
            string strErrors = SilkMarshal.PtrToString((nint)ptrErrors);

            _log.WriteError(strErrors);

            SilkUtil.ReleasePtr(ref pErrorBlob);
        }
    }
}

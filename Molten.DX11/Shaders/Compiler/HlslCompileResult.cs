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

        IDxcUtils* _utils;
        Logger _log;

        ID3D11ShaderReflection* _reflection;
        ShaderDesc* _desc;

        IDxcBlob* _pdbData;
        IDxcBlobUtf16* _pdbPath;

        internal ID3D11ShaderReflection* Reflection => _reflection;

        internal ShaderDesc* Description => _desc;

        internal IDxcBlob* PdbData => _pdbData;

        internal IDxcBlobUtf16* PdbPath => _pdbPath;

        internal HlslCompileResult(IDxcUtils* utils, IDxcResult* result, Logger log)
        {
            _utils = utils;
            _log = log;
            Result = result;

            uint numOutputs = Result->GetNumOutputs();
            for(uint i = 0; i < numOutputs; i++)
            {
                OutKind o = Result->GetOutputByIndex(i);
                switch (o)
                {
                    default:
                    case OutKind.OutNone:
                        break;

                    case OutKind.OutPdb: LoadPdbData(); break;
                    case OutKind.OutReflection: LoadReflection(); break;

                    case OutKind.OutErrors: WriteErrors(); break;
                }
            }
        }

        protected override void OnDispose()
        {
            _reflection->Release();
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
            pData = (IDxcBlob*)pData;
        }

        private void LoadPdbData()
        {
            fixed(IDxcBlobUtf16** pPath = &_pdbPath)
                GetDxcOutput(OutKind.OutPdb, ref _pdbData, pPath);
        }

        private void LoadReflection()
        {
            IDxcBlob* outData = null;
            DxcBuffer* reflectionBuffer = null;
            Guid iid = ID3D11ShaderReflection.Guid;
            void* pReflection = null;

            GetDxcOutput(OutKind.OutReflection, ref outData);
            _utils->CreateReflection(reflectionBuffer, ref iid, ref pReflection);
            _reflection = (ID3D11ShaderReflection*)pReflection;
            _reflection->GetDesc(_desc);
        }
    }
}

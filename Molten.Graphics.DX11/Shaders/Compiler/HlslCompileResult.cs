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
        List<HlslCompilerContext.Message> _messages;

        internal HlslReflection Reflection { get; private set; }

        internal IDxcBlob* PdbData => _pdbData;

        internal string PdbPath { get; private set; }

        internal IDxcBlob* ByteCode => _byteCode;

        internal bool HasErrors { get; private set; }

        internal HlslCompileResult(IDxcUtils* utils, IDxcResult* result)
        {
            _utils = utils;
            _messages = new List<HlslCompilerContext.Message>();
            Messages = _messages.AsReadOnly();
            Result = result;

            LoadByteCode();

            uint numOutputs = Result->GetNumOutputs();
            AddMessage($"{numOutputs} DXC outputs found: ", HlslCompilerContext.Message.Kind.Debug);

            for (uint i = 0; i < numOutputs; i++)
            {
                OutKind o = Result->GetOutputByIndex(i);

                switch (o)
                {
                    default:
                    case OutKind.OutNone:
                        AddMessage($"\t Unsupported output-kind in DXC result: {o}", HlslCompilerContext.Message.Kind.Warning);
                        break;

                    case OutKind.OutPdb: LoadPdbData(); break;
                    case OutKind.OutReflection: LoadReflection(); break;

                    case OutKind.OutErrors: WriteErrors(); break;
                }
            }
        }

        private void AddMessage(string text, HlslCompilerContext.Message.Kind type)
        {
            _messages.Add(new HlslCompilerContext.Message()
            {
                Text = $"[{type}] {text}",
                MessageType = type
            });

            if (type == HlslCompilerContext.Message.Kind.Error)
                HasErrors = true;
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
            AddMessage($"\t Loaded DXC PDB data -- Bytes: {dataSize} -- Path: {PdbPath}", HlslCompilerContext.Message.Kind.Debug);

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
            AddMessage($"\t Loaded DXC Reflection data -- Bytes: {dataSize}", HlslCompilerContext.Message.Kind.Debug);
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

            string[] errors = strErrors.Split('\r', '\n');
            for (int i = 0; i < errors.Length; i++)
                AddMessage(errors[i], HlslCompilerContext.Message.Kind.Error);

            SilkUtil.ReleasePtr(ref pErrorBlob);
        }

        public IReadOnlyList<HlslCompilerContext.Message> Messages { get; private set; }
    }
}

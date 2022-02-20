using Silk.NET.Direct3D11;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ID3D10Blob = Silk.NET.Core.Native.ID3D10Blob;
using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public unsafe class FxcCompileResult : EngineObject, IShaderClassResult 
    {
        public FxcReflection Reflection { get; }

        public ID3D10Blob* ByteCode { get; }

        ID3D10Blob* _byteCode;
        ID3D10Blob* _errors;

        internal FxcCompileResult(ShaderCompilerContext<RendererDX11, HlslFoundation> context,
            D3DCompiler compiler, ID3D10Blob* byteCode, ID3D10Blob* errors)
        {
            _byteCode = byteCode;
            _errors = errors;

            ParseErrors(context);

            if (context.HasErrors)
                return;

            Guid guidReflect = ID3D11ShaderReflection.Guid;
            void* ppReflection = null;

            void* ppByteCode = byteCode->GetBufferPointer();
            nuint numBytes = byteCode->GetBufferSize();

            compiler.Reflect(ppByteCode, numBytes, &guidReflect, &ppReflection);
            Reflection = new FxcReflection((ID3D11ShaderReflection*)ppReflection);
        }

        private void ParseErrors(ShaderCompilerContext<RendererDX11, HlslFoundation> context)
        {
            if (_errors == null)
                return;

            void* ptrErrors = _errors->GetBufferPointer();
            nuint numBytes = _errors->GetBufferSize();
            string strErrors = SilkMarshal.PtrToString((nint)ptrErrors, NativeStringEncoding.UTF8);
            string[] errors = strErrors.Split('\r', '\n');

            for (int i = 0; i < errors.Length; i++)
                context.AddError(errors[i]);

            SilkUtil.ReleasePtr(ref _errors);
        }

        protected override void OnDispose()
        {
            Reflection.Dispose();
            SilkUtil.ReleasePtr(ref _byteCode);
            SilkUtil.ReleasePtr(ref _errors);
        }
    }
}

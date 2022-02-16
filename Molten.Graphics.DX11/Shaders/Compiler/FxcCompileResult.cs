using Silk.NET.Direct3D11;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Molten.Graphics
{
    internal unsafe class FxcCompileResult : ShaderCompileResult<HlslFoundation>
    {
        public FxcReflection Reflection { get; }

        public IDxcBlob* ByteCode { get; }

        IDxcBlob* _byteCode;
        IDxcBlob* _errors;

        internal FxcCompileResult(ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context, 
            D3DCompiler compiler, ShaderSource source, ID3D10Blob* byteCode, ID3D10Blob* errors)
        {
            // TODO: We can temporarily use IDXCBlob since it's an alias of ID3D10Blob. Switch once Silk.NET Correctly implements ID3D10Blob.
            _byteCode = (IDxcBlob*)byteCode;
            _errors = (IDxcBlob*)errors;

            Guid guidReflect = ID3D11ShaderReflection.Guid;
            void* ppReflection = null;

            fixed(void* ptrSource = source.SourceCode)
                compiler.Reflect(ptrSource, source.NumBytes, &guidReflect, &ppReflection);

            Reflection = new FxcReflection((ID3D11ShaderReflection*)ppReflection);
        }

        private void ParseErrors()
        {

        }

        protected override void OnDispose()
        {
            base.OnDispose();

            Reflection.Dispose();
            SilkUtil.ReleasePtr(ref _byteCode);
            SilkUtil.ReleasePtr(ref _errors);
        }
    }
}

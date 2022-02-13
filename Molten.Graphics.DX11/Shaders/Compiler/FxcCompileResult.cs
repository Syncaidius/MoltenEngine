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

        ID3D10Blob* _byteCode;
        ID3D10Blob* _errors;

        internal FxcCompileResult(ShaderCompilerContext<RendererDX11, HlslFoundation, FxcCompileResult> context, 
            D3DCompiler compiler, ShaderSource source, ID3D10Blob* byteCode, ID3D10Blob* errors)
        {
            _byteCode = byteCode;
            _errors = errors;

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

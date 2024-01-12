using Molten.Graphics.Dxc;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;
using System.Reflection;
using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics.DX12;

internal class HlslDxcCompiler : DxcCompiler
{
    public HlslDxcCompiler(RendererDX12 renderer, string includePath, Assembly includeAssembly) : 
        base(renderer, includePath, includeAssembly)
    {
        AddBaseArg(DxcCompilerArg.HlslVersion, "2021");

#if DEBUG
        AddBaseArg(DxcCompilerArg.SkipOptimizations);
        AddBaseArg(DxcCompilerArg.Debug);
#endif
    }

    protected override unsafe void* BuildShader(HlslPass parent, ShaderType type, void* byteCode, nuint numBytes)
    {
        throw new NotImplementedException();
    }

    protected override bool Validate(HlslPass pass, ShaderCompilerContext context, ShaderCodeResult result)
    {
        throw new NotImplementedException();
    }

    protected override unsafe ShaderReflection OnBuildReflection(ShaderCompilerContext context, IDxcBlob* byteCode, DxcBuffer* reflectionBuffer)
    {
        Guid guidReflection = ID3D12ShaderReflection.Guid;
        Guid guidContainer = IDxcContainerReflection.Guid;
        Guid guidClassID = CLSID_DxcContainerReflection;
        void* pReflection = null;

        HResult r = (HResult)Utils->CreateReflection(reflectionBuffer, ref guidReflection, ref pReflection);
        ID3D12ShaderReflection* reflection = (ID3D12ShaderReflection*)pReflection;

        void* pContainer = null;
        r = (HResult)Api.CreateInstance(&guidClassID, &guidContainer, &pContainer);
        IDxcContainerReflection* container = (IDxcContainerReflection*)pContainer;

        // See for part FOURCC values: https://github.com/microsoft/DirectXShaderCompiler/blob/5874b72e81da082ab6ba585dc5f1592c3df27f61/include/dxc/dxcapi.h#L116
        uint partCount = 0;
        r = (HResult)container->GetPartCount(&partCount);
        for (uint i = 0; i < partCount; i++)
        {
            uint pk = 0;
            r = container->GetPartKind(i, &pk);

            // Convert to a part FOURCC. Each letter is 8-bits
            string fourcc = new string((char)((pk >> 0) & 0xFF), 1);
            fourcc += (char)((pk >> 8) & 0xFF);
            fourcc += (char)((pk >> 16) & 0xFF);
            fourcc += (char)((pk >> 24) & 0xFF);
        }

        // TODO populate shader reflection.

        return new ShaderReflection();
    }
}

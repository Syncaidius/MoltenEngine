using Molten.Graphics.Dxc;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;
using System.Reflection;
using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics.DX12;

/// <summary>
/// Compiles HLSL shaders using DXC for Windows.
/// <para>If the compiler fails in release mode, try updating DXIL.dll. Located at: C:\Program Files (x86)\Windows Kits\10\Redist\D3D\x64</para>
/// </summary>
internal unsafe class HlslDxcCompiler : DxcCompiler
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

    protected override unsafe void* BuildNativeShader(HlslPass parent, ShaderType type, void* byteCode, nuint numBytes)
    {
        IDxcBlob* blob = (IDxcBlob*)byteCode;
        byteCode = blob->GetBufferPointer();

        ShaderBytecode* handle = EngineUtil.Alloc<ShaderBytecode>();
        handle->PShaderBytecode = byteCode;
        handle->BytecodeLength = numBytes;

        return handle;
    }

    protected override bool Validate(HlslPass pass, ShaderCompilerContext context, ShaderCodeResult result)
    {
        return true;
    }

    protected override ShaderReflection OnBuildReflection(ShaderCompilerContext context, IDxcBlob* byteCode, DxcBuffer* reflectionBuffer)
    {
        Guid guidReflection = ID3D12ShaderReflection.Guid;
        Guid guidContainer = IDxcContainerReflection.Guid;
        Guid guidClassID = CLSID_DxcContainerReflection;
        void* pReflection = null;

        HResult r = (HResult)Utils->CreateReflection(reflectionBuffer, ref guidReflection, ref pReflection);
        ID3D12ShaderReflection* reflection = (ID3D12ShaderReflection*)pReflection;

        ShaderDesc shaderDesc = new();
        reflection->GetDesc(&shaderDesc);

        ShaderReflection result = new ShaderReflection();
        result.GSInputPrimitive = shaderDesc.GSOutputTopology.FromApi();
        //result.GSMaxOutputVertexCount = shaderDesc.GSMaxOutputVertexCount;

        for(uint i = 0; i < shaderDesc.BoundResources; i++)
        {
            ShaderResourceInfo rInfo = new ShaderResourceInfo()
            {

            };

            result.BoundResources.Add(rInfo);
        }

        PopulateShaderParameters(context, result, reflection, ref shaderDesc, ShaderIOLayoutType.Input);
        PopulateShaderParameters(context, result, reflection, ref shaderDesc, ShaderIOLayoutType.Output);

        NativeUtil.ReleasePtr(ref reflection);
        return result;
    }

    private void PopulateShaderParameters(
        ShaderCompilerContext context, 
        ShaderReflection result, 
        ID3D12ShaderReflection* reflection, 
        ref ShaderDesc desc, 
        ShaderIOLayoutType type)
    {
        List<ShaderParameterInfo> parameters;
        List<SignatureParameterDesc> variables = new List<SignatureParameterDesc>();

        switch (type)
        {
            case ShaderIOLayoutType.Input:
                parameters = result.InputParameters;
                for(uint i = 0; i < desc.InputParameters; i++)
                {
                    SignatureParameterDesc param = new();
                    reflection->GetInputParameterDesc(i, &param);
                    variables.Add(param);
                }
                break;

            case ShaderIOLayoutType.Output:
                parameters = result.OutputParameters;
                for (uint i = 0; i < desc.OutputParameters; i++)
                {
                    SignatureParameterDesc param = new();
                    reflection->GetOutputParameterDesc(i, &param);
                    variables.Add(param);
                }
                break;

            default:
                return;
        }

        for(int i = 0; i < variables.Count; i++)
        {
            SignatureParameterDesc pDesc = variables[i];
            parameters.Add(new ShaderParameterInfo()
            {
                ComponentType = (ShaderRegisterType)pDesc.ComponentType,
                Mask = (ShaderComponentMaskFlags)pDesc.Mask,
                ReadWriteMask = (ShaderComponentMaskFlags)pDesc.ReadWriteMask,
                MinPrecision = (ShaderMinPrecision)pDesc.MinPrecision,
                Register = pDesc.Register,
                SemanticIndex = pDesc.SemanticIndex,
                SemanticName = SilkMarshal.PtrToString((nint)pDesc.SemanticName).ToUpper(),
                SemanticNamePtr = pDesc.SemanticName,
                Stream = pDesc.Stream,
                SystemValueType = (ShaderSVType)pDesc.SystemValueType,
            });
        }
    }
}

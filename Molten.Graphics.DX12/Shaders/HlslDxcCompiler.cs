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

    public override bool BuildStructure(ShaderCompilerContext context, HlslShader shader, ShaderCodeResult result, ShaderComposition composition)
    {
        throw new NotImplementedException();
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
        result.GSMaxOutputVertexCount = shaderDesc.GSMaxOutputVertexCount;

        for(uint i = 0; i < shaderDesc.BoundResources; i++)
        {
            ShaderInputBindDesc rDesc = new();
            reflection->GetResourceBindingDesc(i, &rDesc);

            ShaderResourceInfo bindInfo = new ShaderResourceInfo()
            {
                Name = SilkMarshal.PtrToString((nint)rDesc.Name),
                BindCount = rDesc.BindCount,
                BindPoint = rDesc.BindPoint,
                Dimension = rDesc.Dimension.FromApi(),
                Type = rDesc.Type.FromApi(),
                NumSamples = rDesc.NumSamples,
                ResourceReturnType = rDesc.ReturnType.FromApi(),
                Flags = ((D3DShaderInputFlags)rDesc.UFlags).FromApi(),
            };

            result.BoundResources.Add(bindInfo);

            switch (bindInfo.Type)
            {
                case ShaderInputType.CBuffer:
                    ID3D12ShaderReflectionConstantBuffer* buffer = reflection->GetConstantBufferByName(bindInfo.Name);
                    ShaderBufferDesc bufferDesc = new ShaderBufferDesc();
                    buffer->GetDesc(ref bufferDesc);

                    // Skip binding info buffers
                    if (bufferDesc.Type == D3DCBufferType.D3DCTResourceBindInfo)
                        continue;

                    ConstantBufferInfo cBufferInfo = new ConstantBufferInfo()
                    {
                        Name = bindInfo.Name,
                        Type = bufferDesc.Type.FromApi(),
                        Flags = (ConstantBufferFlags)bufferDesc.UFlags,
                        Size = bufferDesc.Size,
                    };

                    result.ConstantBuffers.Add(bindInfo.Name, cBufferInfo);

                    for (uint v = 0; v < bufferDesc.Variables; v++)
                    {
                        ID3D12ShaderReflectionVariable* variable = buffer->GetVariableByIndex(v);
                        ShaderVariableDesc desc = new ShaderVariableDesc();
                        variable->GetDesc(&desc);

                        ID3D12ShaderReflectionType* rType = variable->GetType();
                        ShaderTypeDesc typeDesc = new ShaderTypeDesc();
                        rType->GetDesc(&typeDesc);

                        ShaderReflection.ReflectionPtr ptrDefault = null;
                        if (desc.DefaultValue != null)
                        {
                            ptrDefault = result.NewPtr(desc.Size);
                            System.Buffer.MemoryCopy(desc.DefaultValue, ptrDefault, desc.Size, desc.Size);
                        }

                        ConstantBufferVariableInfo cVarInfo = new ConstantBufferVariableInfo()
                        {
                            DefaultValue = ptrDefault,
                            Name = SilkMarshal.PtrToString((nint)desc.Name),
                            Size = desc.Size,
                            StartOffset = desc.StartOffset,
                            SamplerSize = desc.SamplerSize,
                            StartSampler = desc.StartSampler,
                            StartTexture = desc.StartTexture,
                            TextureSize = desc.TextureSize,
                            Flags = (ShaderVariableFlags)desc.UFlags,
                        };

                        cBufferInfo.Variables.Add(cVarInfo);
                        cVarInfo.Type.Name = SilkMarshal.PtrToString((nint)typeDesc.Name);
                        cVarInfo.Type.Offset = typeDesc.Offset;
                        cVarInfo.Type.Type = (ShaderVariableType)typeDesc.Type;
                        cVarInfo.Type.Class = (ShaderVariableClass)typeDesc.Class;
                        cVarInfo.Type.ColumnCount = typeDesc.Columns;
                        cVarInfo.Type.RowCount = typeDesc.Rows;
                        cVarInfo.Type.Elements = typeDesc.Elements;
                    }
                    break;
            }
        }

        PopulateReflectionParameters(context, result, reflection, ref shaderDesc, ShaderIOLayoutType.Input);
        PopulateReflectionParameters(context, result, reflection, ref shaderDesc, ShaderIOLayoutType.Output);

        NativeUtil.ReleasePtr(ref reflection);
        return result;
    }

    private void PopulateReflectionParameters(
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

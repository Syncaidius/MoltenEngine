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
        for (int r = 0; r < result.Reflection.BoundResources.Count; r++)
        {
            ShaderResourceInfo bindInfo = result.Reflection.BoundResources[r];
            uint bindPoint = bindInfo.BindPoint;

            switch (bindInfo.Type)
            {
                case ShaderInputType.CBuffer:
                    ConstantBufferInfo bufferInfo = result.Reflection.ConstantBuffers[bindInfo.Name];

                    // Skip binding info buffers
                    if (bufferInfo.Type != ConstantBufferType.ResourceBindInfo)
                    {
                        if (bindPoint >= shader.ConstBuffers.Length)
                            Array.Resize(ref shader.ConstBuffers, (int)bindPoint + 1);

                        if (shader.ConstBuffers[bindPoint] != null && shader.ConstBuffers[bindPoint].BufferName != bindInfo.Name)
                            context.AddMessage($"Shader constant buffer '{shader.ConstBuffers[bindPoint].BufferName}' was overwritten by buffer '{bindInfo.Name}' at the same register (b{bindPoint}).",
                                ShaderCompilerMessage.Kind.Warning);

                        shader.ConstBuffers[bindPoint] = GetConstantBuffer(context, shader, bufferInfo);
                        composition.ConstBufferIds.Add(bindPoint);
                    }

                    break;

                case ShaderInputType.Texture:
                    OnBuildTextureVariable(context, shader, bindInfo);
                    composition.ResourceIds.Add(bindInfo.BindPoint);
                    break;

                case ShaderInputType.Sampler:
                    bool isComparison = bindInfo.HasInputFlags(ShaderInputFlags.ComparisonSampler);
                    ShaderSamplerVariable sampler = GetVariableResource<ShaderSamplerVariable>(context, shader, bindInfo);

                    if (bindPoint >= shader.SamplerVariables.Length)
                    {
                        int oldLength = shader.SamplerVariables.Length;
                        EngineUtil.ArrayResize(ref shader.SamplerVariables, bindPoint + 1);
                        for (int i = oldLength; i < shader.SamplerVariables.Length; i++)
                            shader.SamplerVariables[i] = (i == bindPoint ? sampler : ShaderVariable.Create<ShaderSamplerVariable>(shader, bindInfo.Name));
                    }
                    else
                    {
                        shader.SamplerVariables[bindPoint] = sampler;
                    }
                    composition.SamplerIds.Add(bindPoint);
                    break;

                case ShaderInputType.Structured:
                    ShaderResourceVariable bVar = GetVariableResource<ShaderResourceVariable<GraphicsBuffer>>(context, shader, bindInfo);
                    if (bindPoint >= shader.Resources.Length)
                        EngineUtil.ArrayResize(ref shader.Resources, bindPoint + 1);

                    shader.Resources[bindPoint] = bVar;
                    composition.ResourceIds.Add(bindPoint);
                    break;

                case ShaderInputType.UavRWStructured:
                    OnBuildRWStructuredVariable(context, shader, bindInfo);
                    composition.UnorderedAccessIds.Add(bindPoint);
                    break;

                case ShaderInputType.UavRWTyped:
                    OnBuildRWTypedVariable(context, shader, bindInfo);
                    composition.UnorderedAccessIds.Add(bindPoint);
                    break;
            }
        }

        return true;
    }

    private void OnBuildRWStructuredVariable
        (ShaderCompilerContext context,
        HlslShader shader, ShaderResourceInfo info)
    {
        RWVariable rwBuffer = GetVariableResource<RWVariable<BufferDX12>>(context, shader, info);
        uint bindPoint = info.BindPoint;

        if (bindPoint >= shader.UAVs.Length)
            EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

        shader.UAVs[bindPoint] = rwBuffer;
    }

    private void OnBuildRWTypedVariable(
    ShaderCompilerContext context,
    HlslShader shader, ShaderResourceInfo info)
    {
        RWVariable resource = null;
        uint bindPoint = info.BindPoint;

        switch (info.Dimension)
        {
            case ShaderResourceDimension.Texture1DArray:
            case ShaderResourceDimension.Texture1D:
                resource = GetVariableResource<RWVariable<ITexture1D>>(context, shader, info);
                break;

            case ShaderResourceDimension.Texture2DMS:
            case ShaderResourceDimension.Texture2DMSArray:
            case ShaderResourceDimension.Texture2DArray:
            case ShaderResourceDimension.Texture2D:
                resource = GetVariableResource<RWVariable<ITexture2D>>(context, shader, info);
                break;

            case ShaderResourceDimension.Texture3D:
                resource = GetVariableResource<RWVariable<ITexture3D>>(context, shader, info);
                break;

            case ShaderResourceDimension.TextureCube:
            case ShaderResourceDimension.TextureCubeArray:
                resource = GetVariableResource<RWVariable<ITextureCube>>(context, shader, info);
                break;
        }

        if (bindPoint >= shader.UAVs.Length)
            EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

        // Store the resource variable
        shader.UAVs[bindPoint] = resource;
    }

    private void OnBuildTextureVariable(ShaderCompilerContext context,
        HlslShader shader, ShaderResourceInfo info)
    {
        ShaderResourceVariable obj = null;

        switch (info.Dimension)
        {
            case ShaderResourceDimension.Texture1DArray:
            case ShaderResourceDimension.Texture1D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture1D>>(context, shader, info);
                break;

            case ShaderResourceDimension.Texture2DMS:
            case ShaderResourceDimension.Texture2DMSArray:
            case ShaderResourceDimension.Texture2DArray:
            case ShaderResourceDimension.Texture2D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture2D>>(context, shader, info);
                break;

            case ShaderResourceDimension.Texture3D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture3D>>(context, shader, info);
                break;

            case ShaderResourceDimension.TextureCube:
                obj = GetVariableResource<ShaderResourceVariable<ITextureCube>>(context, shader, info);
                break;
        }

        if (info.BindPoint >= shader.Resources.Length)
            EngineUtil.ArrayResize(ref shader.Resources, info.BindPoint + 1);

        //store the resource variable
        shader.Resources[info.BindPoint] = obj;
    }

    private unsafe ConstantBufferDX12 GetConstantBuffer(ShaderCompilerContext context,
    HlslShader shader, ConstantBufferInfo info)
    {
        ConstantBufferDX12 cBuffer = new ConstantBufferDX12(shader.Device as DeviceDX12, info);
        string localName = cBuffer.BufferName;

        if (cBuffer.BufferName == "$Globals")
            localName += $"_{shader.Name}";

        // Duplication checks.
        if (context.TryGetResource(localName, out ConstantBufferDX12 existing))
        {
            // Check for duplicates
            if (existing != null)
            {
                // Compare buffers. If identical, 
                if (existing.Equals(cBuffer))
                {
                    // Dispose of new buffer, use existing.
                    cBuffer.Dispose();
                    cBuffer = existing;
                }
                else
                {
                    context.AddMessage($"Constant buffers with the same name ('{localName}') do not match. Differing layouts.");
                }
            }
            else
            {
                context.AddMessage($"Constant buffer creation failed. A resource with the name '{localName}' already exists!");
            }
        }
        else
        {
            // Register all of the new buffer's variables
            foreach (GraphicsConstantVariable v in cBuffer.Variables)
            {
                // Check for duplicate variables
                if (shader.Variables.ContainsKey(v.Name))
                {
                    context.AddMessage($"Duplicate variable detected: {v.Name}");
                    continue;
                }

                shader.Variables.Add(v.Name, v);
            }

            // Register the new buffer
            context.AddResource(localName, cBuffer);
        }

        return cBuffer;
    }

    protected T GetVariableResource<T>(ShaderCompilerContext context, HlslShader shader, ShaderResourceInfo info)
    where T : ShaderVariable, new()
    {
        ShaderVariable existing = null;
        T bVar = null;
        Type t = typeof(T);

        if (shader.Variables.TryGetValue(info.Name, out existing))
        {
            T other = existing as T;

            if (other != null)
            {
                // If valid, use existing buffer variable.
                if (other.GetType() == t)
                    bVar = other;
            }
            else
            {
                context.AddMessage($"Resource '{t.Name}' creation failed. A resource with the name '{info.Name}' already exists!");
            }
        }
        else
        {
            bVar = ShaderVariable.Create<T>(shader, info.Name);
            shader.Variables.Add(bVar.Name, bVar);
        }

        return bVar;
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

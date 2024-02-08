using System.Net;

namespace Molten.Graphics;

/// <summary>
/// Responsible for building the variable structure of a <see cref="ShaderComposition"/>.
/// </summary>
internal class ShaderStructureBuilder
{
    internal bool Build(ShaderCompilerContext context, ShaderReflection reflection, ShaderComposition composition, ShaderPassDefinition passDef)
    {
        HlslShader shader = composition.Pass.Parent;

        for (int r = 0; r < reflection.BoundResources.Count; r++)
        {
            ShaderResourceInfo bindInfo = reflection.BoundResources[r];
            uint bindPoint = bindInfo.BindPoint;

            switch (bindInfo.Type)
            {
                case ShaderInputType.CBuffer:
                    ConstantBufferInfo bufferInfo = reflection.ConstantBuffers[bindInfo.Name];

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
                    OnBuildTextureVariable(context, shader, bindInfo, passDef);
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

    private unsafe IConstantBuffer GetConstantBuffer(ShaderCompilerContext context, HlslShader shader, ConstantBufferInfo info)
    {
        string localName = info.Name;

        if (localName == "$Globals")
            localName += $"_{shader.Name}";

        IConstantBuffer cBuffer = context.Compiler.Device.CreateConstantBuffer(info);

        // Duplication checks.
        if (context.TryGetResource(localName, out IConstantBuffer existing))
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
                    context.AddError($"Constant buffers with the same name ('{localName}') do not match. Differing layouts.");
                }
            }
            else
            {
                context.AddError($"Constant buffer creation failed. A resource with the name '{localName}' already exists!");
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

    private void OnBuildTextureVariable(ShaderCompilerContext context, HlslShader shader, ShaderResourceInfo info, ShaderPassDefinition passDef)
    {
        ShaderResourceVariable obj = null;
        GraphicsFormatSupportFlags supportFlags = GraphicsFormatSupportFlags.None;

        switch (info.Dimension)
        {
            case ShaderResourceDimension.Texture1DArray:
            case ShaderResourceDimension.Texture1D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture1D>>(context, shader, info);
                supportFlags |= GraphicsFormatSupportFlags.Texture1D;
                break;

            case ShaderResourceDimension.Texture2DMS:
            case ShaderResourceDimension.Texture2DMSArray:
            case ShaderResourceDimension.Texture2DArray:
            case ShaderResourceDimension.Texture2D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture2D>>(context, shader, info);
                supportFlags |= GraphicsFormatSupportFlags.Texture2D;
                break;

            case ShaderResourceDimension.Texture3D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture3D>>(context, shader, info);
                supportFlags |= GraphicsFormatSupportFlags.Texture3D;
                break;

            case ShaderResourceDimension.TextureCube:
                obj = GetVariableResource<ShaderResourceVariable<ITextureCube>>(context, shader, info);
                supportFlags |= GraphicsFormatSupportFlags.Texturecube;
                break;
        }

        if (passDef.Parameters.Inputs.TryGetValue($"t{info.BindPoint}", out string format))
            obj.ExpectedFormat = shader.Device.GetBestFormat(format, supportFlags);

        if (info.BindPoint >= shader.Resources.Length)
            EngineUtil.ArrayResize(ref shader.Resources, info.BindPoint + 1);
        //store the resource variable
        shader.Resources[info.BindPoint] = obj;
    }

    private void OnBuildRWTypedVariable(ShaderCompilerContext context, HlslShader shader, ShaderResourceInfo info)
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

    private T GetVariableResource<T>(ShaderCompilerContext context, HlslShader shader, ShaderResourceInfo info)
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

    private void OnBuildRWStructuredVariable(ShaderCompilerContext context, HlslShader shader, ShaderResourceInfo info)
    {
        RWVariable rwBuffer = GetVariableResource<RWVariable<GraphicsBuffer>>(context, shader, info);
        uint bindPoint = info.BindPoint;

        if (bindPoint >= shader.UAVs.Length)
            EngineUtil.ArrayResize(ref shader.UAVs, bindPoint + 1);

        shader.UAVs[bindPoint] = rwBuffer;
    }
}

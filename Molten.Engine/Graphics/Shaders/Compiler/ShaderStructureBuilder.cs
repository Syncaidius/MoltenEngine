namespace Molten.Graphics;

/// <summary>
/// Responsible for building the variable structure of a <see cref="ShaderPassStage"/>.
/// </summary>
internal class ShaderStructureBuilder
{
    internal bool Build(ShaderCompilerContext context, ShaderReflection reflection, ShaderPassStage stage, ShaderPassDefinition passDef)
    {
        Shader shader = stage.Pass.Parent;

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
                        IConstantBuffer cb = GetConstantBuffer(context, shader, bufferInfo);
                        stage.AddBinding(cb, bindPoint);
                    }

                    break;

                case ShaderInputType.Texture:
                    OnBuildTextureVariable(context, stage, bindInfo, passDef);
                    break;

                case ShaderInputType.Sampler:
                    //bool isComparison = bindInfo.HasInputFlags(ShaderInputFlags.ComparisonSampler);

                    // If a sampler definition is not found for the current sampler bind-point, use a default sampler.
                    if (!passDef.Samplers.TryGetValue(bindPoint.ToString(), out ShaderSamplerParameters samplerParams)
                        && !passDef.Samplers.TryGetValue(bindInfo.Name, out samplerParams))
                    {
                        context.AddWarning($"Sampler '{bindInfo.Name}' was not defined in the shader pass '{passDef.Name}'. Using default sampler.");
                        samplerParams = new ShaderSamplerParameters(SamplerPreset.Default);
                        shader.LinkSampler(samplerParams);

                    }

                    stage.AddBinding(samplerParams.LinkedSampler, bindPoint);
                    break;

                case ShaderInputType.Structured:
                    ShaderResourceVariable bVar = GetVariableResource<ShaderResourceVariable<GraphicsBuffer>>(context, stage, bindInfo);
                    stage.AddBinding(bVar, bindPoint);
                    break;

                case ShaderInputType.UavRWStructured:
                    RWVariable rwBuffer = GetVariableResource<RWVariable<GraphicsBuffer>>(context, stage, bindInfo);
                    stage.AddBinding(rwBuffer, bindInfo.BindPoint);
                    break;

                case ShaderInputType.UavRWTyped:
                    OnBuildRWTypedVariable(context, stage, bindInfo);
                    break;
            }
        }

        return true;
    }

    private unsafe IConstantBuffer GetConstantBuffer(ShaderCompilerContext context, Shader shader, ConstantBufferInfo info)
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

    private void OnBuildTextureVariable(ShaderCompilerContext context, ShaderPassStage stage, ShaderResourceInfo info, ShaderPassDefinition passDef)
    {
        ShaderResourceVariable obj = null;
        GraphicsFormatSupportFlags supportFlags = GraphicsFormatSupportFlags.None;

        switch (info.Dimension)
        {
            case ShaderResourceDimension.Texture1DArray:
            case ShaderResourceDimension.Texture1D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture1D>>(context, stage, info);
                supportFlags |= GraphicsFormatSupportFlags.Texture1D;
                break;

            case ShaderResourceDimension.Texture2DMS:
            case ShaderResourceDimension.Texture2DMSArray:
            case ShaderResourceDimension.Texture2DArray:
            case ShaderResourceDimension.Texture2D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture2D>>(context, stage, info);
                supportFlags |= GraphicsFormatSupportFlags.Texture2D;
                break;

            case ShaderResourceDimension.Texture3D:
                obj = GetVariableResource<ShaderResourceVariable<ITexture3D>>(context, stage, info);
                supportFlags |= GraphicsFormatSupportFlags.Texture3D;
                break;

            case ShaderResourceDimension.TextureCube:
                obj = GetVariableResource<ShaderResourceVariable<ITextureCube>>(context, stage, info);
                supportFlags |= GraphicsFormatSupportFlags.Texturecube;
                break;
        }

        // Set the expected format.
        if (passDef.Parameters.Formats.TryGetValue($"t{info.BindPoint}", out GraphicsFormat format))
        {
            if (!stage.Device.IsFormatSupported(format, supportFlags))
            {
                GraphicsFormatSupportFlags supported = stage.Device.GetFormatSupport(format);
                context.AddError($"Format 't{info.BindPoint}' not supported for texture ('{info.Name}') in pass '{passDef.Name}'");
                context.AddError($"\tFormat: {format}");
                context.AddError($"\tSupported: {supported}");
                context.AddError($"\tRequired support: {supportFlags}");
            }
            else
            {
                obj.ExpectedFormat = format;
            }
        }

        stage.AddBinding(obj, info.BindPoint);
    }

    private void OnBuildRWTypedVariable(ShaderCompilerContext context, ShaderPassStage stage, ShaderResourceInfo info)
    {
        RWVariable resource = null;
        uint bindPoint = info.BindPoint;

        switch (info.Dimension)
        {
            case ShaderResourceDimension.Texture1DArray:
            case ShaderResourceDimension.Texture1D:
                resource = GetVariableResource<RWVariable<ITexture1D>>(context, stage, info);
                break;

            case ShaderResourceDimension.Texture2DMS:
            case ShaderResourceDimension.Texture2DMSArray:
            case ShaderResourceDimension.Texture2DArray:
            case ShaderResourceDimension.Texture2D:
                resource = GetVariableResource<RWVariable<ITexture2D>>(context, stage, info);
                break;

            case ShaderResourceDimension.Texture3D:
                resource = GetVariableResource<RWVariable<ITexture3D>>(context, stage, info);
                break;

            case ShaderResourceDimension.TextureCube:
            case ShaderResourceDimension.TextureCubeArray:
                resource = GetVariableResource<RWVariable<ITextureCube>>(context, stage, info);
                break;
        }

        stage.AddBinding(resource, bindPoint);
    }

    private T GetVariableResource<T>(ShaderCompilerContext context, ShaderPassStage stage, ShaderResourceInfo info)
        where T : ShaderVariable, new()
    {
        Shader shader = stage.Pass.Parent;
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
            bVar = shader.CreateVariable<T>(info.Name, info.BindPoint, 0); // TODO - bind space
            shader.Variables.Add(bVar.Name, bVar);
        }

        return bVar;
    }
}

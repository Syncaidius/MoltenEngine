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
                        ShaderResourceVariable<IConstantBuffer> bufferVar = stage.Bindings.Create<ShaderResourceVariable<IConstantBuffer>>(bufferInfo.Name, 
                            bindPoint, 0, ShaderBindType.ConstantBuffer);

                        stage.Bindings.Add(ShaderBindType.ConstantBuffer, bufferVar, bindPoint);
                        bufferVar.DefaultValue = cb;
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

                    stage.Bindings.Add(samplerParams.LinkedSampler, bindPoint);
                    break;

                case ShaderInputType.Structured:
                    BuildResourceVariable<ShaderResourceVariable<GraphicsBuffer>>(context, stage, bindInfo, ShaderBindType.Resource);
                    break;

                case ShaderInputType.UavRWStructured:
                    BuildResourceVariable<RWVariable<GraphicsBuffer>>(context, stage, bindInfo, ShaderBindType.UnorderedAccess);
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
                obj = BuildResourceVariable<ShaderResourceVariable<ITexture1D>>(context, stage, info, ShaderBindType.Resource);
                supportFlags |= GraphicsFormatSupportFlags.Texture1D;
                break;

            case ShaderResourceDimension.Texture2DMS:
            case ShaderResourceDimension.Texture2DMSArray:
            case ShaderResourceDimension.Texture2DArray:
            case ShaderResourceDimension.Texture2D:
                obj = BuildResourceVariable<ShaderResourceVariable<ITexture2D>>(context, stage, info, ShaderBindType.Resource);
                supportFlags |= GraphicsFormatSupportFlags.Texture2D;
                break;

            case ShaderResourceDimension.Texture3D:
                obj = BuildResourceVariable<ShaderResourceVariable<ITexture3D>>(context, stage, info, ShaderBindType.Resource);
                supportFlags |= GraphicsFormatSupportFlags.Texture3D;
                break;

            case ShaderResourceDimension.TextureCube:
                obj = BuildResourceVariable<ShaderResourceVariable<ITextureCube>>(context, stage, info, ShaderBindType.Resource);
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
    }

    private void OnBuildRWTypedVariable(ShaderCompilerContext context, ShaderPassStage stage, ShaderResourceInfo info)
    {
        switch (info.Dimension)
        {
            case ShaderResourceDimension.Texture1DArray:
            case ShaderResourceDimension.Texture1D:
                BuildResourceVariable<RWVariable<ITexture1D>>(context, stage, info, ShaderBindType.UnorderedAccess);
                break;

            case ShaderResourceDimension.Texture2DMS:
            case ShaderResourceDimension.Texture2DMSArray:
            case ShaderResourceDimension.Texture2DArray:
            case ShaderResourceDimension.Texture2D:
                BuildResourceVariable<RWVariable<ITexture2D>>(context, stage, info, ShaderBindType.UnorderedAccess);
                break;

            case ShaderResourceDimension.Texture3D:
                BuildResourceVariable<RWVariable<ITexture3D>>(context, stage, info, ShaderBindType.UnorderedAccess);
                break;

            case ShaderResourceDimension.TextureCube:
            case ShaderResourceDimension.TextureCubeArray:
                BuildResourceVariable<RWVariable<ITextureCube>>(context, stage, info, ShaderBindType.UnorderedAccess);
                break;
        }
    }

    private T BuildResourceVariable<T>(ShaderCompilerContext context, ShaderPassStage stage, ShaderResourceInfo info, ShaderBindType bindType)
        where T : ShaderResourceVariable, new()
    {
        if (stage.Pass.Parent.Variables.TryGetValue(info.Name, out ShaderVariable existing))
        {
            if (existing is T resVariable)
            {
                stage.Bindings.Add(bindType, resVariable, info.BindPoint, 0);
                return resVariable;
            }
            else
            {
                context.AddMessage($"Resource variable '{typeof(T).Name}' creation failed. A diffrent variable ('{existing.GetType().Name}') with the name '{info.Name}' already exists!");
            }
        }
        else
        {
            return stage.Bindings.Create<T>(info.Name, info.BindPoint, 0, bindType); // TODO - bind space
        }

        return null;
    }
}

using System.Reflection;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    internal unsafe abstract class FxcClassCompiler : 
        ShaderClassCompiler
    {
        protected bool HasConstantBuffer(ShaderCompilerContext context, 
            HlslShader shader, string bufferName, string[] varNames)
        {
            foreach (ShaderConstantBuffer buffer in shader.ConstBuffers)
            {
                if (buffer == null)
                    continue;

                if (buffer.BufferName == bufferName)
                {
                    if (buffer.Variables.Length != varNames.Length)
                    {
                        context.AddMessage($"Shader '{bufferName}' constant buffer does not have the correct number of variables ({varNames.Length})", 
                            ShaderCompilerMessage.Kind.Error);
                        return false;
                    }

                    for (int i = 0; i < buffer.Variables.Length; i++)
                    {
                        ShaderConstantVariable variable = buffer.Variables[i];
                        string expectedName = varNames[i];

                        if (variable.Name != expectedName)
                        {
                            context.AddMessage($"Shader '{bufferName}' constant variable #{i + 1} is incorrect: Named '{variable.Name}' instead of '{expectedName}'",
                                ShaderCompilerMessage.Kind.Error);
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        protected ShaderIOStructure BuildIO(ShaderClassResult result, ShaderIOStructureType type)
        {
            List<ShaderParameterInfo> parameters;

            switch (type)
            {
                case ShaderIOStructureType.Input:
                    parameters = result.Reflection.InputParameters;
                    break;

                case ShaderIOStructureType.Output:
                    parameters = result.Reflection.OutputParameters;
                    break;

                default:
                    return null;
            }

            int count = parameters.Count;
            ShaderIOStructureDX11 structure = new ShaderIOStructureDX11((uint)count);

            for (int i = 0; i < count; i++)
            {
                ShaderParameterInfo pDesc = parameters[i];

                InputElementDesc el = new InputElementDesc()
                {
                    SemanticName = (byte*)pDesc.SemanticNamePtr,
                    SemanticIndex = pDesc.SemanticIndex,
                    InputSlot = 0, // This does not need to be set. A shader has a single layout, 
                    InstanceDataStepRate = 0, // This does not need to be set. The data is set via Context.DrawInstanced + vertex data/layout.
                    AlignedByteOffset = 16 * pDesc.Register,
                    InputSlotClass = InputClassification.PerVertexData,
                };

                ShaderComponentMaskFlags usageMask = (pDesc.Mask & ShaderComponentMaskFlags.ComponentX);
                usageMask |= (pDesc.Mask & ShaderComponentMaskFlags.ComponentY);
                usageMask |= (pDesc.Mask & ShaderComponentMaskFlags.ComponentZ);
                usageMask |= (pDesc.Mask & ShaderComponentMaskFlags.ComponentW);

                ShaderRegisterType comType = pDesc.ComponentType;
                switch (usageMask)
                {
                    case ShaderComponentMaskFlags.ComponentX:
                        if (comType == ShaderRegisterType.UInt32)
                            el.Format = Format.FormatR32Uint;
                        else if (comType == ShaderRegisterType.SInt32)
                            el.Format = Format.FormatR32Sint;
                        else if (comType == ShaderRegisterType.Float32)
                            el.Format = Format.FormatR32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY:
                        if (comType == ShaderRegisterType.UInt32)
                            el.Format = Format.FormatR32G32Uint;
                        else if (comType == ShaderRegisterType.SInt32)
                            el.Format = Format.FormatR32G32Sint;
                        else if (comType == ShaderRegisterType.Float32)
                            el.Format = Format.FormatR32G32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY |
                ShaderComponentMaskFlags.ComponentZ:
                        if (comType == ShaderRegisterType.UInt32)
                            el.Format = Format.FormatR32G32B32Uint;
                        else if (comType == ShaderRegisterType.SInt32)
                            el.Format = Format.FormatR32G32B32Sint;
                        else if (comType == ShaderRegisterType.Float32)
                            el.Format = Format.FormatR32G32B32Float;
                        break;

                    case ShaderComponentMaskFlags.ComponentX | ShaderComponentMaskFlags.ComponentY |
                ShaderComponentMaskFlags.ComponentZ | ShaderComponentMaskFlags.ComponentW:
                        if (comType == ShaderRegisterType.UInt32)
                            el.Format = Format.FormatR32G32B32A32Uint;
                        else if (comType == ShaderRegisterType.SInt32)
                            el.Format = Format.FormatR32G32B32A32Sint;
                        else if (comType == ShaderRegisterType.Float32)
                            el.Format = Format.FormatR32G32B32A32Float;
                        break;
                }

                // Store the element
                structure.Elements[i] = el;
                structure.Metadata[i] = new ShaderIOStructure.InputElementMetadata()
                {
                    Name = pDesc.SemanticName,
                    SemanticIndex = pDesc.SemanticIndex,
                    SystemValueType = pDesc.SystemValueType
                };
            }

            return structure;
        }

        protected bool BuildStructure(ShaderCompilerContext context, 
            HlslShader shader, ShaderClassResult result, ShaderComposition composition) 
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

                            if(shader.ConstBuffers[bindPoint] != null && shader.ConstBuffers[bindPoint].BufferName != bindInfo.Name)
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
                                shader.SamplerVariables[i] = (i ==  bindPoint ? sampler : new ShaderSamplerVariable(shader));
                        }
                        else
                        {
                            shader.SamplerVariables[bindPoint] = sampler;
                        }
                        composition.SamplerIds.Add(bindPoint);
                        break;

                    case ShaderInputType.Structured:
                        BufferVariable bVar = GetVariableResource<BufferVariable>(context, shader, bindInfo);
                        if (bindPoint >= shader.Resources.Length)
                            EngineUtil.ArrayResize(ref shader.Resources, bindPoint + 1);

                        shader.Resources[bindPoint] = bVar;
                        composition.ResourceIds.Add(bindPoint);
                        break;

                    default:
                        OnBuildVariableStructure(context, shader, result, bindInfo);
                        break;
                }
            }

            return true;
        }

        protected abstract void OnBuildVariableStructure(ShaderCompilerContext context, 
            HlslFoundation shader, ShaderClassResult result, ShaderResourceInfo info);

        private void OnBuildTextureVariable(ShaderCompilerContext context, 
            HlslShader shader, ShaderResourceInfo info)
        {
            ShaderResourceVariable obj = null;

            switch (info.Dimension)
            {
                case ShaderResourceDimension.Texture1DArray:
                case ShaderResourceDimension.Texture1D:
                    obj = GetVariableResource<Texture1DVariable>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texture2DMS:
                case ShaderResourceDimension.Texture2DMSArray:
                case ShaderResourceDimension.Texture2DArray:
                case ShaderResourceDimension.Texture2D:
                    obj = GetVariableResource<Texture2DVariable>(context, shader, info);
                    break;

                case ShaderResourceDimension.Texturecube:
                    obj = GetVariableResource<TextureCubeVariable>(context, shader, info);
                    break;
            }

            if (info.BindPoint >= shader.Resources.Length)
                EngineUtil.ArrayResize(ref shader.Resources, info.BindPoint + 1);

            //store the resource variable
            shader.Resources[info.BindPoint] = obj;
        }

        private unsafe ShaderConstantBuffer GetConstantBuffer(ShaderCompilerContext context, 
            HlslShader shader, ConstantBufferInfo info)
        {
            ShaderConstantBuffer cBuffer = new ShaderConstantBuffer(shader.Device as DeviceDX11, BufferMode.DynamicDiscard, info);
            string localName = cBuffer.BufferName;

            if (cBuffer.BufferName == "$Globals")
                localName += $"_{shader.Name}";

            // Duplication checks.
            if (context.TryGetResource(localName, out ShaderConstantBuffer existing))
            {
                // Check for duplicates
                if (existing != null)
                {
                    // Compare buffers. If identical, 
                    if (existing.Hash == cBuffer.Hash)
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
                foreach (ShaderConstantVariable v in cBuffer.Variables)
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

        protected T GetVariableResource<T>(ShaderCompilerContext context, 
            HlslShader shader, ShaderResourceInfo info) 
            where T : class, IShaderValue
        {
            IShaderValue existing = null;
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
                BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                bVar = Activator.CreateInstance(typeof(T), bindFlags, null, new object[] { shader }, null) as T;
                bVar.Name = info.Name;

                shader.Variables.Add(bVar.Name, bVar);
            }

            return bVar;
        }        
    }
}

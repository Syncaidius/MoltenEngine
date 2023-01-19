using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System.Reflection;

namespace Molten.Graphics
{
    internal unsafe abstract class FxcClassCompiler : 
        ShaderClassCompiler<RendererDX11, HlslFoundation>
    {
        protected bool HasConstantBuffer(ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
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

        protected void BuildIO(ShaderClassResult result, ShaderComposition composition)
        {
            composition.InputStructure = new ShaderIOStructure(result, ShaderIOStructureType.Input);
            composition.OutputStructure = new ShaderIOStructure(result, ShaderIOStructureType.Output);
        }

        protected bool BuildStructure<T>(ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            HlslShader shader, ShaderClassResult result, ShaderComposition<T> composition) 
            where T : unmanaged
        {
            for (int r = 0; r < result.Reflection.Inputs.Count; r++)
            {
                ShaderInputInfo bindInfo = result.Reflection.Inputs[r];
                uint bindPoint = bindInfo.BindPoint;

                switch (bindInfo.InputType)
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

        protected abstract void OnBuildVariableStructure(ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            HlslFoundation shader, ShaderClassResult result, ShaderInputInfo info);

        private void OnBuildTextureVariable(ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            HlslShader shader, ShaderInputInfo info)
        {
            ShaderResourceVariable obj = null;
            uint bindPoint = info.BindPoint;

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

            if (bindPoint >= shader.Resources.Length)
                EngineUtil.ArrayResize(ref shader.Resources, bindPoint + 1);

            //store the resource variable
            shader.Resources[bindPoint] = obj;
        }

        private unsafe ShaderConstantBuffer GetConstantBuffer(ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            HlslShader shader, ConstantBufferInfo info)
        {
            ShaderConstantBuffer cBuffer = new ShaderConstantBuffer(shader.NativeDevice, BufferMode.DynamicDiscard, info);
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

        protected T GetVariableResource<T>(ShaderCompilerContext<RendererDX11, HlslFoundation> context, 
            HlslShader shader, ShaderInputInfo info) 
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

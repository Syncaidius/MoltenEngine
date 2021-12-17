using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe delegate T* HlslCreateShaderCallback<T>(HlslCompileResult result, HlslShader shader) where T: unmanaged;
    internal unsafe abstract class HlslSubCompiler
    {

#if RELEASE
         HlslCompilerArg _compileFlags = HlslCompilerArg.OptimizationLevel3;
#else
        HlslCompilerArg _compileFlags = HlslCompilerArg.WarningsAreErrors;
#endif

        internal abstract List<IShader> Parse(ShaderCompilerContext context, RendererDX11 renderer, string header);

        protected bool HasResource(HlslShader shader, string resourceName)
        {
            foreach (ShaderResourceVariable resource in shader.Resources)
            {
                if (resource == null)
                    continue;

                if (resource.Name == resourceName)
                    return true;
            }

            return false;
        }

        protected bool HasConstantBuffer(ShaderCompilerContext context, HlslShader shader, string bufferName, string[] varNames)
        {
            foreach (ShaderConstantBuffer buffer in shader.ConstBuffers)
            {
                if (buffer == null)
                    continue;

                if (buffer.BufferName == bufferName)
                {
                    if (buffer.Variables.Length != varNames.Length)
                    {
                        context.Errors.Add($"Material '{bufferName}' constant buffer does not have the correct number of variables ({varNames.Length})");
                        return false;
                    }

                    for (int i = 0; i < buffer.Variables.Length; i++)
                    {
                        ShaderConstantVariable variable = buffer.Variables[i];
                        string expectedName = varNames[i];

                        if (variable.Name != expectedName)
                        {
                            context.Errors.Add($"Material '{bufferName}' constant variable #{i + 1} is incorrect: Named '{variable.Name}' instead of '{expectedName}'");
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        protected void BuildIO(HlslCompileResult result, ShaderComposition composition)
        {
            composition.InputStructure = new ShaderIOStructure(result, ShaderIOStructureType.Input);
            composition.OutputStructure = new ShaderIOStructure(result, ShaderIOStructureType.Output);
        }

        protected bool BuildStructure<T>(ShaderCompilerContext context, HlslShader shader, 
            HlslCompileResult result, ShaderComposition<T> composition, HlslCreateShaderCallback<T> createCallback) 
            where T : unmanaged
        {
            for (uint r = 0; r < result.Reflection.Desc->BoundResources; r++)
            {
                HlslInputBindDescription bindDesc = result.Reflection.BindDescs[r];
                uint bindPoint = bindDesc.Ptr->BindPoint;

                switch (bindDesc.Ptr->Type)
                {
                    case D3DShaderInputType.D3DSitCbuffer:
                        ID3D11ShaderReflectionConstantBuffer* buffer = result.Reflection.Ptr->GetConstantBufferByName(bindDesc.Ptr->Name);
                        ShaderBufferDesc* bufferDesc = null;
                        buffer->GetDesc(bufferDesc);

                        // Skip binding info buffers
                        if (bufferDesc->Type != D3DCBufferType.D3DCTResourceBindInfo)
                        {
                            if (bindPoint >= shader.ConstBuffers.Length)
                                Array.Resize(ref shader.ConstBuffers, (int)bindPoint + 1);

                            if(shader.ConstBuffers[bindPoint] != null && shader.ConstBuffers[bindPoint].BufferName != bindDesc.Name)
                                context.Messages.Add($"Material constant buffer '{shader.ConstBuffers[bindPoint].BufferName}' was overwritten by buffer '{bindDesc.Name}' at the same register (b{bindPoint}).");

                            shader.ConstBuffers[bindPoint] = GetConstantBuffer(context, shader, buffer);
                            composition.ConstBufferIds.Add(bindPoint);
                        }

                        break;

                    case D3DShaderInputType.D3DSitTexture:
                        OnBuildTextureVariable(context, shader, bindDesc);
                        composition.ResourceIds.Add(bindDesc.Ptr->BindPoint);
                        break;

                    case D3DShaderInputType.D3DSitSampler:
                        bool isComparison = ((D3DShaderInputFlags)bindDesc.Ptr->UFlags & D3DShaderInputFlags.D3DSifComparisonSampler) == 
                            D3DShaderInputFlags.D3DSifComparisonSampler;

                        ShaderSamplerVariable sampler = GetVariableResource<ShaderSamplerVariable>(context, shader, bindDesc);

                        if (bindPoint >= shader.SamplerVariables.Length)
                        {
                            int oldLength = shader.SamplerVariables.Length;
                            EngineInterop.ArrayResize(ref shader.SamplerVariables, bindPoint + 1);
                            for (int i = oldLength; i < shader.SamplerVariables.Length; i++)
                                shader.SamplerVariables[i] = (i ==  bindPoint ? sampler : new ShaderSamplerVariable(shader));
                        }
                        else
                        {
                            shader.SamplerVariables[bindPoint] = sampler;
                        }
                        composition.SamplerIds.Add(bindPoint);
                        break;

                    case D3DShaderInputType.D3DSitStructured:
                        BufferVariable bVar = GetVariableResource<BufferVariable>(context, shader, bindDesc);
                        if (bindPoint >= shader.Resources.Length)
                            EngineInterop.ArrayResize(ref shader.Resources, bindPoint + 1);

                        shader.Resources[bindPoint] = bVar;
                        composition.ResourceIds.Add(bindPoint);
                        break;

                    default:
                        OnBuildVariableStructure(context, shader, result, bindDesc);
                        break;
                }
            }

            // TODO retrieve compiled shader for result.ShaderBytecode
            composition.RawShader = createCallback(result, shader);
            return true;
        }

        protected abstract void OnBuildVariableStructure(ShaderCompilerContext context, 
            HlslShader shader, HlslCompileResult result, 
            HlslInputBindDescription bind);


        private void OnBuildTextureVariable(ShaderCompilerContext context, HlslShader shader, HlslInputBindDescription bind)
        {
            ShaderResourceVariable obj = null;
            uint bindPoint = bind.Ptr->BindPoint;

            switch (bind.Ptr->Dimension)
            {
                case D3DSrvDimension.D3DSrvDimensionTexture1Darray:
                case D3DSrvDimension.D3DSrvDimensionTexture1D:
                    obj = GetVariableResource<Texture1DVariable>(context, shader, bind);
                    break;

                case D3DSrvDimension.D3DSrvDimensionTexture2Darray:
                case D3DSrvDimension.D3DSrvDimensionTexture2D:
                    obj = GetVariableResource<Texture2DVariable>(context, shader, bind);
                    break;

                case D3DSrvDimension.D3DSrvDimensionTexturecube:
                    obj = GetVariableResource<TextureCubeVariable>(context, shader, bind);
                    break;
            }

            if (bindPoint >= shader.Resources.Length)
                EngineInterop.ArrayResize(ref shader.Resources, bindPoint + 1);

            //store the resource variable
            shader.Resources[bindPoint] = obj;
        }

        private unsafe ShaderConstantBuffer GetConstantBuffer(ShaderCompilerContext context, HlslShader shader,
            ID3D11ShaderReflectionConstantBuffer* buffer)
        {
            ShaderBufferDesc* bufferDesc = null;
            buffer->GetDesc(bufferDesc);

            ShaderConstantBuffer cBuffer = new ShaderConstantBuffer(shader.Device, BufferMode.DynamicDiscard, buffer, bufferDesc);
            string localName = cBuffer.BufferName;

            if (cBuffer.BufferName == "$Globals")
                localName += $"_{shader.Name}";

            // Duplication checks.
            if (context.ConstantBuffers.TryGetValue(localName, out ShaderConstantBuffer existing))
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
                        LogHlslMessage(context, string.Format("Constant buffers with the same name ('{0}') do not match. Differing layouts.", localName));
                    }
                }
                else
                {
                    LogHlslMessage(context, string.Format("Constant buffer creation failed. A resource with the name '{0}' already exists!", localName));
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
                        LogHlslMessage(context, "Duplicate variable detected: " + v.Name);
                        continue;
                    }

                    shader.Variables.Add(v.Name, v);
                }

                // Register the new buffer
                context.ConstantBuffers.Add(localName, cBuffer);
            }

            return cBuffer;
        }

        protected T GetVariableResource<T>(ShaderCompilerContext context, HlslShader shader, HlslInputBindDescription desc) where T : class, IShaderValue
        {
            IShaderValue existing = null;
            T bVar = null;
            Type t = typeof(T);

            if (shader.Variables.TryGetValue(desc.Name, out existing))
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
                    LogHlslMessage(context, string.Format("Resource '{0}' creation failed. A resource with the name '{1}' already exists!", t.Name, desc.Name));
                }
            }
            else
            {
                bVar = Activator.CreateInstance(typeof(T), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { shader }, null) as T;
                bVar.Name = desc.Name;

                shader.Variables.Add(bVar.Name, bVar);
            }

            return bVar;
        }

        /// <summary>Compiles HLSL source code and outputs the result. Returns true if successful, or false if there were errors.</summary>
        /// <param name="log"></param>
        /// <param name="entryPoint"></param>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="filename"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected bool Compile(string entryPoint, ShaderType type, ShaderCompilerContext context, out HlslCompileResult result)
        {
            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.HlslShaders.TryGetValue(entryPoint, out result))
            {
                string strProfile = ShaderModel.Model5_0.ToProfile(type);
                result = ShaderBytecode.Compile(context.Source, entryPoint, strProfile, _compileFlags, EffectFlags.None, context.Filename);

                if (result.Message != null)
                {
                    LogHlslMessage(context, $"Material Pass ({entryPoint}) -- {result.Message}");
                    if (result.Message.Contains("error")) // NOTE: Workaround for SharpDX 4.0.1 where .HasErrors appears broken.
                        return false;
                }

                context.HlslShaders.Add(entryPoint, result);
            }

            return !result.HasErrors;
        }

        protected void LogHlslMessage(ShaderCompilerContext context, string txt)
        {
            string[] lines = txt.Split(HlslCompiler.NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string msg = string.IsNullOrWhiteSpace(context.Filename) ? lines[i] : (context.Filename + ": " + lines[i]);
                if (lines[i].Contains("error"))
                    context.Errors.Add(msg);
                else
                    context.Messages.Add(msg);
            }
        }
    }
}

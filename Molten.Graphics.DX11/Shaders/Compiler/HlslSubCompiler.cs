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
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    internal unsafe abstract class HlslSubCompiler
    {

#if RELEASE
         HlslCompilerArg _compileFlags = HlslCompilerArg.OptimizationLevel3;
#else
        HlslCompilerArg _compileFlags = HlslCompilerArg.WarningsAreErrors;
#endif

        internal abstract List<IShader> Parse(HlslCompilerContext context, RendererDX11 renderer, string header);

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

        protected bool HasConstantBuffer(HlslCompilerContext context, HlslShader shader, string bufferName, string[] varNames)
        {
            foreach (ShaderConstantBuffer buffer in shader.ConstBuffers)
            {
                if (buffer == null)
                    continue;

                if (buffer.BufferName == bufferName)
                {
                    if (buffer.Variables.Length != varNames.Length)
                    {
                        context.AddMessage($"Material '{bufferName}' constant buffer does not have the correct number of variables ({varNames.Length})", 
                            HlslCompilerContext.Message.Kind.Error);
                        return false;
                    }

                    for (int i = 0; i < buffer.Variables.Length; i++)
                    {
                        ShaderConstantVariable variable = buffer.Variables[i];
                        string expectedName = varNames[i];

                        if (variable.Name != expectedName)
                        {
                            context.AddMessage($"Material '{bufferName}' constant variable #{i + 1} is incorrect: Named '{variable.Name}' instead of '{expectedName}'",
                                HlslCompilerContext.Message.Kind.Error);
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

        protected bool BuildStructure<T>(HlslCompilerContext context, HlslShader shader, 
            HlslCompileResult result, ShaderComposition<T> composition) 
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
                        ShaderBufferDesc bufferDesc = new ShaderBufferDesc();
                        buffer->GetDesc(ref bufferDesc);

                        // Skip binding info buffers
                        if (bufferDesc.Type != D3DCBufferType.D3DCTResourceBindInfo)
                        {
                            if (bindPoint >= shader.ConstBuffers.Length)
                                Array.Resize(ref shader.ConstBuffers, (int)bindPoint + 1);

                            if(shader.ConstBuffers[bindPoint] != null && shader.ConstBuffers[bindPoint].BufferName != bindDesc.Name)
                                context.AddMessage($"Material constant buffer '{shader.ConstBuffers[bindPoint].BufferName}' was overwritten by buffer '{bindDesc.Name}' at the same register (b{bindPoint}).", 
                                    HlslCompilerContext.Message.Kind.Warning);

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

                    case D3DShaderInputType.D3DSitStructured:
                        BufferVariable bVar = GetVariableResource<BufferVariable>(context, shader, bindDesc);
                        if (bindPoint >= shader.Resources.Length)
                            EngineUtil.ArrayResize(ref shader.Resources, bindPoint + 1);

                        shader.Resources[bindPoint] = bVar;
                        composition.ResourceIds.Add(bindPoint);
                        break;

                    default:
                        OnBuildVariableStructure(context, shader, result, bindDesc);
                        break;
                }
            }

            return true;
        }

        protected abstract void OnBuildVariableStructure(HlslCompilerContext context, 
            HlslShader shader, HlslCompileResult result, 
            HlslInputBindDescription bind);


        private void OnBuildTextureVariable(HlslCompilerContext context, HlslShader shader, HlslInputBindDescription bind)
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
                EngineUtil.ArrayResize(ref shader.Resources, bindPoint + 1);

            //store the resource variable
            shader.Resources[bindPoint] = obj;
        }

        private unsafe ShaderConstantBuffer GetConstantBuffer(HlslCompilerContext context, HlslShader shader,
            ID3D11ShaderReflectionConstantBuffer* buffer)
        {
            ShaderBufferDesc bufferDesc = new ShaderBufferDesc();
            buffer->GetDesc(ref bufferDesc);

            ShaderConstantBuffer cBuffer = new ShaderConstantBuffer(shader.Device, BufferMode.DynamicDiscard, buffer, ref bufferDesc);
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
                context.ConstantBuffers.Add(localName, cBuffer);
            }

            return cBuffer;
        }

        protected T GetVariableResource<T>(HlslCompilerContext context, HlslShader shader, HlslInputBindDescription desc) where T : class, IShaderValue
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
                    context.AddMessage($"Resource '{t.Name}' creation failed. A resource with the name '{desc.Name}' already exists!");
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
        protected bool Compile(string entryPoint, ShaderType type, HlslCompilerContext context, out HlslCompileResult result)
        {
            // Since it's not possible to have two functions in the same file with the same name, we'll just check if
            // a shader with the same entry-point name is already loaded in the context.
            if (!context.HlslShaders.TryGetValue(entryPoint, out result))
            {
                string strProfile = ShaderModel.Model5_0.ToProfile(type);
                string argString = context.Args.ToString();
                uint argCount = context.Args.Count;
                char** ptrArgString = context.Args.GetArgsPtr();

                Guid dxcResultGuid = IDxcResult.Guid;
                void* dxcResult;
                uint numBytes = (uint)(sizeof(char) * context.Source.Length);

                IDxcBlobEncoding* srcEncoding;
                void* ptrSource = (void*)SilkMarshal.StringToPtr(context.Source, NativeStringEncoding.LPWStr);
                context.Compiler.Utils->CreateBlob(ptrSource, numBytes, DXC.CPUtf16, &srcEncoding);

                Buffer srcBuffer = new Buffer()
                {
                    Ptr = srcEncoding->GetBufferPointer(),
                    Size = srcEncoding->GetBufferSize(),
                    Encoding = 0
                };

     
                context.Compiler.Native->Compile(&srcBuffer, ptrArgString, argCount, context.Includer, &dxcResultGuid, &dxcResult);
                result = new HlslCompileResult(context, (IDxcResult*)dxcResult);

                // Free allocated resource.
                srcEncoding->Release();
                SilkMarshal.Free((nint)ptrArgString);
                SilkMarshal.Free((nint)ptrSource);

                if(context.HasErrors)
                    return false;

                context.HlslShaders.Add(entryPoint, result);
            }

            return true;
        }
    }
}

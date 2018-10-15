using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Molten.Graphics
{
    internal abstract class HlslSubCompiler
    {

#if RELEASE
         ShaderFlags _compileFlags = ShaderFlags.OptimizationLevel3;
#else
        ShaderFlags _compileFlags = ShaderFlags.WarningsAreErrors;
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

        protected ShaderReflection BuildIo(CompilationResult code, ShaderComposition composition)
        {
            ShaderReflection shaderRef = new ShaderReflection(code);
            ShaderDescription desc = shaderRef.Description;
            composition.InputStructure = new ShaderIOStructure(shaderRef, ref desc, ShaderIOStructureType.Input);
            composition.OutputStructure = new ShaderIOStructure(shaderRef, ref desc, ShaderIOStructureType.Output);

            return shaderRef;
        }

        protected bool BuildStructure<T>(ShaderCompilerContext context, HlslShader shader, ShaderReflection shaderRef, CompilationResult code, ShaderComposition<T> composition) 
            where T : DeviceChild
        {
            //build variable data
            ShaderDescription desc = shaderRef.Description;

            for (int r = 0; r < desc.BoundResources; r++)
            {
                InputBindingDescription binding = shaderRef.GetResourceBindingDescription(r);
                int bindPoint = binding.BindPoint;
                switch (binding.Type)
                {
                    case ShaderInputType.ConstantBuffer:
                        ConstantBuffer buffer = shaderRef.GetConstantBuffer(binding.Name);

                        // Skip binding info buffers
                        if (buffer.Description.Type != ConstantBufferType.ResourceBindInformation)
                        {
                            if (bindPoint >= shader.ConstBuffers.Length)
                                Array.Resize(ref shader.ConstBuffers, bindPoint + 1);

                            if(shader.ConstBuffers[bindPoint] != null && shader.ConstBuffers[bindPoint].BufferName != binding.Name)
                                context.Messages.Add($"Material constant buffer '{shader.ConstBuffers[bindPoint].BufferName}' was overwritten by buffer '{binding.Name}' at the same register (b{bindPoint}).");

                            shader.ConstBuffers[bindPoint] = GetConstantBuffer(context, shader, buffer);
                            composition.ConstBufferIds.Add(bindPoint);
                        }

                        break;

                    case ShaderInputType.Texture:
                        OnBuildTextureVariable(context, shader, binding);
                        composition.ResourceIds.Add(binding.BindPoint);
                        break;

                    case ShaderInputType.Sampler:
                        bool isComparison = (binding.Flags & ShaderInputFlags.ComparisonSampler) == ShaderInputFlags.ComparisonSampler;
                        ShaderSamplerVariable sampler = GetVariableResource<ShaderSamplerVariable>(context, shader, binding);

                        if (bindPoint >= shader.SamplerVariables.Length)
                        {
                            int oldLength = shader.SamplerVariables.Length;
                            Array.Resize(ref shader.SamplerVariables, bindPoint + 1);
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
                        BufferVariable bVar = GetVariableResource<BufferVariable>(context, shader, binding);
                        if (bindPoint >= shader.Resources.Length)
                            Array.Resize(ref shader.Resources, bindPoint + 1);

                        shader.Resources[bindPoint] = bVar;
                        composition.ResourceIds.Add(bindPoint);
                        break;

                    default:
                        OnBuildVariableStructure(context, shader, shaderRef, binding, binding.Type);
                        break;
                }

            }

            composition.RawShader = Activator.CreateInstance(typeof(T), shader.Device.D3d, code.Bytecode.Data, null) as T;
            return true;
        }

        protected abstract void OnBuildVariableStructure(ShaderCompilerContext context, HlslShader shader, ShaderReflection reflection, InputBindingDescription binding, ShaderInputType inputType);


        private void OnBuildTextureVariable(ShaderCompilerContext context, HlslShader shader, InputBindingDescription binding)
        {
            ShaderResourceVariable obj = null;
            int bindPoint = binding.BindPoint;

            switch (binding.Dimension)
            {
                case ShaderResourceViewDimension.Texture1DArray:
                case ShaderResourceViewDimension.Texture1D:
                    obj = GetVariableResource<Texture1DVariable>(context, shader, binding);
                    break;

                case ShaderResourceViewDimension.Texture2DArray:
                case ShaderResourceViewDimension.Texture2D:
                    obj = GetVariableResource<Texture2DVariable>(context, shader, binding);
                    break;

                case ShaderResourceViewDimension.TextureCube:
                    obj = GetVariableResource<TextureCubeVariable>(context, shader, binding);
                    break;
            }

            if (bindPoint >= shader.Resources.Length)
                Array.Resize(ref shader.Resources, bindPoint + 1);

            //store the resource variable
            shader.Resources[bindPoint] = obj;
        }

        private ShaderConstantBuffer GetConstantBuffer(ShaderCompilerContext context, HlslShader shader, ConstantBuffer buffer)
        {
            ShaderConstantBuffer cBuffer = new ShaderConstantBuffer(shader.Device, BufferMode.DynamicDiscard, buffer);
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
                //register all of the new buffer's variables
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

        protected T GetVariableResource<T>(ShaderCompilerContext context, HlslShader shader, InputBindingDescription desc) where T : class, IShaderValue
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
        protected bool Compile(string entryPoint, ShaderType type, ShaderCompilerContext context, out CompilationResult result)
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

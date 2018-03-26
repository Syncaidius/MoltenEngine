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
        static string[] _newLine = new string[] { "\r", "\n", "\r\n", Environment.NewLine };

        Dictionary<string, ShaderNodeParser> _parsers = new Dictionary<string, ShaderNodeParser>();

#if RELEASE
         ShaderFlags _compileFlags = ShaderFlags.OptimizationLevel3;
#else
        ShaderFlags _compileFlags = ShaderFlags.WarningsAreErrors;
#endif

        Logger _log;

        internal HlslSubCompiler(Logger log)
        {
            _log = log;
            AddParser<ShaderNameParser>("name");
            AddParser<MaterialDescParser>("description");
            AddParser<MaterialAuthorParser>("author");
        }

        protected void AddParser<T>(string nodeName) where T : ShaderNodeParser
        {
            nodeName = nodeName.ToLower();
            T parser = Activator.CreateInstance(typeof(T), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { nodeName }, null) as T;
            _parsers.Add(nodeName, parser);
        }

        internal abstract ShaderParseResult Parse(RendererDX11 renderer, ShaderCompilerContext context);

        protected void ParseHeader(HlslShader shader, ShaderCompilerContext context)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(context.Header);

            XmlNode rootNode = doc.ChildNodes[0];
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                string nodeName = node.Name.ToLower();

                switch (nodeName)
                {
                    case "rasterizer":
                        shader.RasterizerState = context.Compiler.RasterizerParser.Parse(shader, context, node);
                        break;

                    default:
                        ShaderNodeParser parser = null;
                        if (_parsers.TryGetValue(nodeName, out parser))
                            parser.Parse(shader, context, node);
                        break;
                }

            }
        }

        protected ShaderReflection BuildIo(CompilationResult code, ShaderComposition composition)
        {
            ShaderReflection shaderRef = new ShaderReflection(code);
            ShaderDescription desc = shaderRef.Description;
            composition.InputStructure = new ShaderIOStructure(shaderRef, ref desc, ShaderIOStructureType.Input);
            composition.OutputStructure = new ShaderIOStructure(shaderRef, ref desc, ShaderIOStructureType.Output);

            return shaderRef;
        }

        protected bool BuildStructure<T>(HlslShader shader, ShaderReflection shaderRef, CompilationResult code, ShaderComposition<T> composition) 
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
                            ShaderConstantBuffer cBuffer = GetConstantBuffer(shader, BufferMode.Dynamic, buffer);

                            if (bindPoint >= shader.ConstBuffers.Length)
                                Array.Resize(ref shader.ConstBuffers, bindPoint + 1);

                            shader.ConstBuffers[bindPoint] = cBuffer;
                            composition.ConstBufferIds.Add(bindPoint);
                        }

                        break;

                    case ShaderInputType.Texture:
                        OnBuildTextureVariable(shader, binding);
                        composition.ResourceIds.Add(binding.BindPoint);
                        break;

                    case ShaderInputType.Sampler:
                        bool isComparison = (binding.Flags & ShaderInputFlags.ComparisonSampler) == ShaderInputFlags.ComparisonSampler;

                        ShaderSamplerVariable sampler = GetVariableResource<ShaderSamplerVariable>(shader, binding);

                        if (bindPoint >= shader.Samplers.Length)
                            Array.Resize(ref shader.Samplers, bindPoint + 1);

                        shader.Samplers[bindPoint] = sampler;
                        composition.SamplerIds.Add(bindPoint);
                        break;

                    case ShaderInputType.Structured:
                        BufferVariable bVar = GetVariableResource<BufferVariable>(shader, binding);
                        if (bindPoint >= shader.Resources.Length)
                            Array.Resize(ref shader.Resources, bindPoint + 1);

                        shader.Resources[bindPoint] = bVar;
                        composition.ResourceIds.Add(bindPoint);
                        break;

                    default:
                        OnBuildVariableStructure(shader, shaderRef, binding, binding.Type);
                        break;
                }

            }

            //VertexShader d3dShader = new VertexShader();
            //T rawShader = Activator.CreateInstance(typeof(T), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { shader.Device.D3d, code.Bytecode.Data, null }) as T;
            // If we've reached this far, instanciate the DX11 shader object
            //composition.RawShader = rawShader;

            composition.RawShader = Activator.CreateInstance(typeof(T), shader.Device.D3d, code.Bytecode.Data, null) as T;

            return true;
        }

        protected abstract void OnBuildVariableStructure(HlslShader shader, ShaderReflection reflection, InputBindingDescription binding, ShaderInputType inputType);


        private void OnBuildTextureVariable(HlslShader shader, InputBindingDescription binding)
        {
            ShaderResourceVariable obj = null;
            int bindPoint = binding.BindPoint;

            switch (binding.Dimension)
            {
                case ShaderResourceViewDimension.Texture1D:
                    obj = GetVariableResource<Texture1DVariable>(shader, binding);
                    break;

                case ShaderResourceViewDimension.Texture2D:
                    obj = GetVariableResource<Texture2DVariable>(shader, binding);
                    break;

                case ShaderResourceViewDimension.TextureCube:
                    obj = GetVariableResource<TextureCubeVariable>(shader, binding);
                    break;

                case ShaderResourceViewDimension.Texture1DArray:
                    obj = GetVariableResource<TextureArray1DVariable>(shader, binding);
                    break;

                case ShaderResourceViewDimension.Texture2DArray:
                    obj = GetVariableResource<TextureArray2DVariable>(shader, binding);
                    break;
            }

            if (bindPoint >= shader.Resources.Length)
                Array.Resize(ref shader.Resources, bindPoint + 1);

            //store the resource variable
            shader.Resources[bindPoint] = obj;
        }

        private ShaderConstantBuffer GetConstantBuffer(HlslShader shader, BufferMode mode, ConstantBuffer buffer)
        {
            ShaderConstantBuffer cBuffer = new ShaderConstantBuffer(shader, BufferMode.Dynamic, buffer);
            PipelineShaderObject existing = null;

            // Duplication checks.
            if (shader.ResourcePool.TryGetValue(cBuffer.BufferName, out existing))
            {
                ShaderConstantBuffer other = existing as ShaderConstantBuffer;

                // Check for duplicates
                if (other != null)
                {
                    // Compare buffers. If identical, 
                    if (other.Hash == cBuffer.Hash)
                    {
                        // Dispose of new buffer, use existing.
                        cBuffer.Dispose();
                        cBuffer = other;
                    }
                    else
                    {
                        LogHlslMessage(string.Format("Constant buffers with the same name ('{0}') do not match. Differing layouts.", cBuffer.BufferName));
                    }
                }
                else
                {
                    LogHlslMessage(string.Format("Constant buffer creation failed. A resource with the name '{0}' already exists!", cBuffer.BufferName));
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
                        LogHlslMessage("Duplicate variable detected: " + v.Name);
                        continue;
                    }

                    shader.Variables.Add(v.Name, v);
                }

                // Register the new buffer
                shader.ResourcePool.Add(cBuffer.BufferName, cBuffer);
            }

            return cBuffer;
        }

        protected T GetVariableResource<T>(HlslShader shader, InputBindingDescription desc) where T : class, IShaderValue
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
                    LogHlslMessage(string.Format("Resource '{0}' creation failed. A resource with the name '{1}' already exists!", t.Name, desc.Name));
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
                    LogHlslMessage($"Material Pass ({entryPoint}) -- {result.Message}", context.Filename);
                    if (result.Message.Contains("error")) // NOTE: Workaround for SharpDX 4.0.1 where .HasErrors appears broken.
                        return false;
                }

                context.HlslShaders.Add(entryPoint, result);
            }

            return !result.HasErrors;
        }

        protected void LogHlslMessage(string txt, string filename = null)
        {
            string[] lines = txt.Split(_newLine, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                if (string.IsNullOrWhiteSpace(filename))
                    _log.WriteLine("[SHADER] " + lines[i]);
                else
                    _log.WriteLine("[SHADER] " + filename + ": " + lines[i]);
            }
        }
    }
}

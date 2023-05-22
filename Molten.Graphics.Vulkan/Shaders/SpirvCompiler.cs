using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Molten.Graphics.Dxc;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;
using SpirvReflector;
using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics.Vulkan
{
    internal class SpirvCompiler : DxcCompiler
    {
        Vk _vk;
        ReflectionLogAdapter _logger;

        public SpirvCompiler(Vk vk, RenderService renderer, string includePath, Assembly includeAssembly, VersionVK targetApi) : 
            base(renderer, includePath, includeAssembly)
        {
            _vk = vk;
            _logger = new ReflectionLogAdapter(renderer.Log);

            AddBaseArg(DxcCompilerArg.SpirV);
            AddBaseArg(DxcCompilerArg.HlslVersion, "2021");
            AddBaseArg(DxcCompilerArg.VulkanVersion, $"vulkan{targetApi.Major}.{targetApi.Minor}");
            AddBaseArg(DxcCompilerArg.Debug);
            AddBaseArg(DxcCompilerArg.SpirVReflection);
        }

        protected override unsafe void* BuildShader(HlslPass parent, ShaderType type, void* byteCode, nuint numBytes)
        {
            IDxcBlob* blob = (IDxcBlob*)byteCode;
            byteCode = blob->GetBufferPointer();

            ShaderModuleCreateInfo info = new ShaderModuleCreateInfo(StructureType.ShaderModuleCreateInfo);
            info.CodeSize = numBytes;
            info.PCode = (uint*)byteCode;
            info.Flags = ShaderModuleCreateFlags.None;

            string fn = parent.Parent.Filename.Replace('.', '_').Replace('/', '_').Replace('\\', '_');
            using (FileStream stream = new FileStream($"test_{fn}.spirv", FileMode.Create, FileAccess.Write))
            {
                Span<byte> t = new Span<byte>(byteCode, (int)numBytes);
                stream.Write(t.ToArray(), 0, (int)numBytes);
            }

            DeviceVK device = parent.Device as DeviceVK;
            ShaderModule* shader = EngineUtil.Alloc<ShaderModule>();
            Result r = _vk.CreateShaderModule(device, info, null, shader);
            if (!r.Check(device, () => $"Failed to create {type} shader module"))
                EngineUtil.Free(ref shader);

            return shader;
        }

        protected override unsafe ShaderReflection OnBuildReflection(ShaderCompilerContext context, IDxcBlob* byteCode, DxcBuffer* reflectionBuffer)
        {
            SpirvReflection reflection = new SpirvReflection(_logger);
            SpirvReflectionResult rr = reflection.Reflect(byteCode->GetBufferPointer(), byteCode->GetBufferSize());

            ShaderReflection result = new ShaderReflection()
            {
                GSInputPrimitive = GeometryHullTopology.Triangle, // TODO populate
            };

            //using (FileStream stream = new FileStream(context.Source.Filename + ".spirv", FileMode.Create, FileAccess.Write))
            //{
            //    using (BinaryWriter writer = new BinaryWriter(stream))
            //    {
            //        Span<byte> t = new Span<byte>(byteCode->GetBufferPointer(), (int)byteCode->GetBufferSize());
            //        writer.Write(t.ToArray(), 0, (int)byteCode->GetBufferSize());
            //    }
            //}

            // TODO get input/output resource bindings

            // Populate input/output resource parameters
            foreach (SpirvEntryPoint ep in rr.EntryPoints)
            {
                PopulateReflectionParamters(result, ep, ShaderIOStructureType.Input);
                PopulateReflectionParamters(result, ep, ShaderIOStructureType.Output);

                break; //  TODO support multiple entry points in reflection
            }

            return result;
        }

        private void PopulateReflectionParamters(ShaderReflection result, SpirvEntryPoint ep, ShaderIOStructureType type)
        {
            List<ShaderParameterInfo> parameters;
            IReadOnlyList<SpirvVariable> variables;

            switch (type)
            {
                case ShaderIOStructureType.Input:
                    variables = ep.Inputs;
                    parameters = result.InputParameters;
                    break;

                case ShaderIOStructureType.Output:
                    variables = ep.Outputs;
                    parameters = result.OutputParameters;
                    break;

                default:
                    return;
            }

            for (int i = 0; i < variables.Count; i++)
            {
                SpirvVariable v = variables[i];

                ShaderParameterInfo p = new ShaderParameterInfo()
                {
                    ComponentType = ShaderRegisterType.UInt32,
                    Mask = ShaderComponentMaskFlags.All,
                    ReadWriteMask = 255,
                    MinPrecision = GetMinPrecision(v.Type),
                    Register = v.Binding,
                    SemanticIndex = v.Binding,
                    Stream = 0,
                };

                ProcessDecorations(p, v);
                p.SystemValueType = GetSystemValue(p.SemanticName);
                parameters.Add(p);
            }
        }

        private ShaderMinPrecision GetMinPrecision(SpirvType type)
        {
            switch (type.Kind)
            {
                case SpirvTypeKind.Int:
                    switch (type.NumBytes)
                    {
                        default: return ShaderMinPrecision.Default;
                        case 2: return ShaderMinPrecision.Sint16;
                        case 10: return ShaderMinPrecision.Any10;
                    }

                case SpirvTypeKind.Float:
                    switch (type.NumBytes)
                    {
                        default: throw new Exception($"Unsupported float length: {type.NumBytes}");
                        case 2: return ShaderMinPrecision.Float16;
                        case 10: return ShaderMinPrecision.Float28;
                    }

                case SpirvTypeKind.UInt:
                    switch (type.NumBytes)
                    {
                        default: return ShaderMinPrecision.Default;
                        case 2: return ShaderMinPrecision.Uint16;
                        case 10: return ShaderMinPrecision.Any10;
                    }

                default:
                    return ShaderMinPrecision.Default;
            }  
        }

        private unsafe void ProcessDecorations(ShaderParameterInfo p, SpirvVariable v)
        {
            // Prioritize user semantics
            foreach (SpirvDecoration dec in v.Decorations.Keys)
            {
                IReadOnlyList<object> parameters = v.Decorations[dec];

                switch (dec)
                {
                    case SpirvDecoration.UserSemantic:
                        p.SemanticName = parameters[0] as string;
                        break;
                }
            }

            // Now for built-in semantics.
            if (string.IsNullOrWhiteSpace(p.SemanticName))
            {
                foreach (SpirvDecoration dec in v.Decorations.Keys)
                {
                    IReadOnlyList<object> parameters = v.Decorations[dec];

                    switch (dec)
                    {
                        case SpirvDecoration.BuiltIn:
                            SpirvBuiltIn bi = (SpirvBuiltIn)parameters[0];
                            p.SemanticName = BuiltInToSemantic(bi);
                            break;
                    }
                }
            }

            p.SemanticNamePtr = (void*)SilkMarshal.StringToPtr(p.SemanticName, NativeStringEncoding.UTF8);
        }

        private string BuiltInToSemantic(SpirvBuiltIn builtIn)
        {
            switch (builtIn)
            {
                default:
                    return $"GL_{builtIn.ToString().ToUpper()}";

                case SpirvBuiltIn.VertexId:
                    return "SV_VERTEXID";
            }
        }

        private ShaderSVType GetSystemValue(string semanticName)
        {
            if (!semanticName.StartsWith("SV_") || !Enum.TryParse(semanticName.Substring(3), out ShaderSVType result))
                return ShaderSVType.Undefined;

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Molten.Graphics.Dxc;
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
            SpirvReflectionResult result = reflection.Reflect(byteCode->GetBufferPointer(), byteCode->GetBufferSize());

            //using (FileStream stream = new FileStream(context.Source.Filename + ".spirv", FileMode.Create, FileAccess.Write))
            //{
            //    using (BinaryWriter writer = new BinaryWriter(stream))
            //    {
            //        Span<byte> t = new Span<byte>(byteCode->GetBufferPointer(), (int)byteCode->GetBufferSize());
            //        writer.Write(t.ToArray(), 0, (int)byteCode->GetBufferSize());
            //    }
            //}

            throw new NotImplementedException();
        }
    }
}

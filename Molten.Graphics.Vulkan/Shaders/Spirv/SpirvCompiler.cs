using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Molten.Graphics.Dxc;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;
using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics.Vulkan
{
    internal class SpirvCompiler : DxcCompiler
    {
        Vk _vk;

        public SpirvCompiler(Vk vk, RenderService renderer, string includePath, Assembly includeAssembly, VersionVK targetApi) : 
            base(renderer, includePath, includeAssembly)
        {
            _vk = vk;
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
            SpirvReflector reflector = new SpirvReflector(byteCode->GetBufferPointer(), byteCode->GetBufferSize(), Log);
            // TODO Add support for pre-compiled shaders.
            // TODO Build external tool for running khronos spirv-reflect tool alongside our own SpirVCompiler to generate a .mcfx (molten compiled fx) file.
            // TODO Store ShaderReflection object as json inside the .mcfx file.

            // TODO also consider simply running DXC in DX12 mode in the external tool to remove dependency on spirv-reflect. This would only work on windows for now however.
            throw new NotImplementedException();
        }
    }
}

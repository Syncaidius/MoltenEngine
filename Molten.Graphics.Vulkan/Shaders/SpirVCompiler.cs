using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics.Dxc;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class SpirVCompiler : DxcCompiler
    {
        Vk _vk;

        public SpirVCompiler(Vk vk, RenderService renderer, string includePath, Assembly includeAssembly, VersionVK targetApi) : 
            base(renderer, includePath, includeAssembly)
        {
            _vk = vk;
            AddBaseArg(DxcCompilerArg.SpirV);
            AddBaseArg(DxcCompilerArg.HlslVersion, "2021");
            AddBaseArg(DxcCompilerArg.VulkanVersion, $"vulkan{targetApi.Major}.{targetApi.Minor}");
            AddBaseArg(DxcCompilerArg.Debug);
        }

        protected override unsafe void* BuildShader(HlslPass parent, ShaderType type, void* byteCode, nuint numBytes)
        {
            IDxcBlob* blob = (IDxcBlob*)byteCode;
            byteCode = blob->GetBufferPointer();

            ShaderModuleCreateInfo info = new ShaderModuleCreateInfo(StructureType.ShaderModuleCreateInfo);
            info.CodeSize = numBytes;
            info.PCode = (uint*)byteCode;
            info.Flags = ShaderModuleCreateFlags.None;

            DeviceVK device = parent.Device as DeviceVK;
            ShaderModule* shader = EngineUtil.Alloc<ShaderModule>();
            Result r = _vk.CreateShaderModule(device, info, null, shader);
            if (!r.Check(device, () => $"Failed to create {type} shader module"))
                EngineUtil.Free(ref shader);

            return shader;
        }

        protected override ShaderReflection OnBuildDxcReflection(ShaderCompilerContext context, ref Silk.NET.Direct3D.Compilers.Buffer reflectionBuffer)
        {
            throw new NotImplementedException();
        }
    }
}

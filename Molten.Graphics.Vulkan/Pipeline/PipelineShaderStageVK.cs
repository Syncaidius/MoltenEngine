using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class PipelineShaderStageVK : GraphicsObject
    {
        StructKey<PipelineShaderStageCreateInfo> Desc;

        public unsafe PipelineShaderStageVK(GraphicsDevice device, ShaderType type, string entryPoint) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<PipelineShaderStageCreateInfo>();
            ref PipelineShaderStageCreateInfo desc = ref Desc.Value;
            desc.SType = StructureType.PipelineShaderStageCreateInfo;
            desc.PName = (byte*)SilkMarshal.StringToPtr(entryPoint, NativeStringEncoding.UTF8);

            switch (type)
            {
                case ShaderType.Vertex: desc.Stage = ShaderStageFlags.VertexBit; break;
                case ShaderType.Hull: desc.Stage = ShaderStageFlags.TessellationControlBit;break;
                case ShaderType.Domain: desc.Stage = ShaderStageFlags.TessellationEvaluationBit; break;
                case ShaderType.Geometry: desc.Stage = ShaderStageFlags.GeometryBit; break;
                case ShaderType.Pixel: desc.Stage = ShaderStageFlags.FragmentBit; break;
                case ShaderType.Compute: desc.Stage = ShaderStageFlags.ComputeBit;break;
            }
        }

        public unsafe override void GraphicsRelease()
        {
            byte* ptrName = Desc.Value.PName;
            Desc.Dispose();
            SilkMarshal.FreeString((nint)ptrName);
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            throw new NotImplementedException();
        }
    }
}

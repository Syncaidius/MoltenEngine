using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a managed device context pipeline stage.
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    internal unsafe abstract class PipeShaderStage : PipeStage
    {
        internal PipeShaderStage(PipeDX11 pipe, ShaderType shaderType) :
           base(pipe, shaderType.ToStageType())
        {
            ShaderStageType = shaderType;

            uint maxSamplers = pipe.Device.Features.MaxSamplerSlots;
            Samplers = DefineSlotGroup<ShaderSampler>(maxSamplers, PipeBindTypeFlags.Input, "Sampler");

            uint maxResources = pipe.Device.Features.MaxInputResourceSlots;
            Resources = DefineSlotGroup<PipeBindableResource>(maxResources, PipeBindTypeFlags.Input, "Resource");

            uint maxCBuffers = pipe.Device.Features.MaxConstantBufferSlots;
            ConstantBuffers = DefineSlotGroup<ShaderConstantBuffer>(maxCBuffers, PipeBindTypeFlags.Input, "C-Buffer");

            Shader = DefineSlot<HlslShader>(0, PipeBindTypeFlags.Input, "Shader");
        }

        protected abstract void OnBind();

        /// <summary>
        /// Gets the slots for binding <see cref="ShaderSampler"/> to the current <see cref="PipeShaderStage"/>.
        /// </summary>
        internal PipeBindSlotGroup<PipeSampler> Samplers { get; }

        internal PipeBindSlotGroup<PipeBindableResource> Resources { get; }

        internal PipeBindSlotGroup<ShaderConstantBuffer> ConstantBuffers { get; }

        /// <summary>
        /// Gets the shader bind slot for the current <see cref="PipeShaderStage{T, S}"/>
        /// </summary>
        internal PipeBindSlot<HlslShader> Shader { get; }

        public ShaderType ShaderStageType { get; }
    }
}

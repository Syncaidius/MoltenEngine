using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ContextShaderState<T> : ContextStateProvider
        where T : unmanaged
    {
        public ContextShaderState(DeviceContextState parent, DeviceContext context, ShaderType shaderType) : base(parent)
        {
            ContextShaderStage stage = context.Shaders[shaderType];

            uint maxSamplers = context.Device.Features.MaxSamplerSlots;
            Samplers = parent.RegisterSlotGroup(PipeBindTypeFlags.Input, "Sampler", maxSamplers, new SamplerGroupBinder(stage));

            uint maxResources = context.Device.Features.MaxInputResourceSlots;
            Resources = parent.RegisterSlotGroup(PipeBindTypeFlags.Input, "Resource", maxResources, new ResourceGroupBinder(stage));

            uint maxCBuffers = context.Device.Features.MaxConstantBufferSlots;
            ConstantBuffers = parent.RegisterSlotGroup(PipeBindTypeFlags.Input, "C-Buffer", maxCBuffers, new CBufferGroupBinder(stage));

            Shader = parent.RegisterSlot<ShaderComposition<T>>(PipeBindTypeFlags.Input, "Shader", 0);
        }

        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }

        internal override void Bind(DeviceContextState state, DeviceContext context)
        {
            throw new NotImplementedException();
        }

        /// Gets the slots for binding <see cref="ShaderSampler"/> to the current <see cref="PipeShaderStage"/>.
        /// </summary>
        internal ContextSlotGroup<ShaderSampler> Samplers { get; }

        /// <summary>
        /// Gets the slots for binding <see cref="PipeBindableResource"/> to the current <see cref="PipeShaderStage"/>.
        /// </summary>
        internal ContextSlotGroup<PipeBindableResource> Resources { get; }

        /// <summary>
        /// Gets the slots for binding <see cref="ShaderConstantBuffer"/> to the current <see cref="PipeShaderStage"/>/
        /// </summary>
        internal ContextSlotGroup<ShaderConstantBuffer> ConstantBuffers { get; }

        /// <summary>
        /// Gets the shader bind slot for the current <see cref="PipeShaderStage{T, S}"/>
        /// </summary>
        internal ContextSlot<ShaderComposition<T>> Shader { get; }

        /// <summary>
        /// Gets the type of shader stage that the current <see cref="PipeShaderStage"/> represents.
        /// </summary>
        public ShaderType ShaderStageType { get; }
    }
}

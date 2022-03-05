using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe abstract class ContextShaderStage<T>
        where T : unmanaged
    {
        internal ContextShaderStage(DeviceContextState state, ShaderType type)
        {
            Context = state.Context;
            Type = type;

            uint maxSamplers = Context.Device.Features.MaxSamplerSlots;
            Samplers = state.RegisterSlotGroup(PipeBindTypeFlags.Input, "Sampler", maxSamplers, new SamplerGroupBinder<T>(this));

            uint maxResources = Context.Device.Features.MaxInputResourceSlots;
            Resources = state.RegisterSlotGroup(PipeBindTypeFlags.Input, "Resource", maxResources, new ResourceGroupBinder<T>(this));

            uint maxCBuffers = Context.Device.Features.MaxConstantBufferSlots;
            ConstantBuffers = state.RegisterSlotGroup(PipeBindTypeFlags.Input, "C-Buffer", maxCBuffers, new CBufferGroupBinder<T>(this));

            Shader = state.RegisterSlot(PipeBindTypeFlags.Input, "Shader", 0, new ShaderSlotBinder<T>(this));
        }

        internal virtual bool Bind()
        {
            Samplers.BindAll();
            Resources.BindAll();
            ConstantBuffers.BindAll();

            return Shader.Bind();
        }

        internal abstract void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states);

        internal abstract void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView** views);

        internal abstract void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers);

        internal abstract void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances);

        internal DeviceContext Context { get; }

        internal ShaderType Type { get; }


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
    }
}

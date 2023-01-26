using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe abstract class ContextShaderStage<T>
        where T : unmanaged
    {
        internal ContextShaderStage(CommandQueueDX11 queue, ShaderType type)
        {
            Context = queue;
            Type = type;

            GraphicsCapabilities cap = Context.DXDevice.Adapter.Capabilities;
            ShaderStageCapabilities shaderCap = cap[type];
            
            Samplers = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_Sampler", cap.MaxShaderSamplers, new SamplerGroupBinder<T>(this));
            Resources = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_Resource", shaderCap.MaxInResources, new ResourceGroupBinder<T>(this));
            ConstantBuffers = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_C-Buffer", cap.ConstantBuffers.MaxSlots, new CBufferGroupBinder<T>(this));
            Shader = queue.RegisterSlot(GraphicsBindTypeFlags.Input, $"{type}_Shader", 0, new ShaderSlotBinder<T>(this));
        }

        internal virtual bool Bind()
        {
            bool shaderChanged = Shader.Bind();

            ShaderComposition<T> composition = Shader.BoundValue;

            if (composition.PtrShader != null)
            {
                // Apply pass constant buffers to slots
                for (int i = 0; i < composition.ConstBufferIds.Count; i++)
                {
                    uint slotID = composition.ConstBufferIds[i];
                    ConstantBuffers[slotID].Value = composition.Parent.ConstBuffers[slotID];
                }

                // Apply pass resources to slots
                for (int i = 0; i < composition.ResourceIds.Count; i++)
                {
                    uint slotID = composition.ResourceIds[i];
                    Resources[slotID].Value = composition.Parent.Resources[slotID]?.Resource;
                }

                // Apply pass samplers to slots
                for (int i = 0; i < composition.SamplerIds.Count; i++)
                {
                    uint slotID = composition.SamplerIds[i];
                    Samplers[slotID].Value = composition.Parent.SamplerVariables[slotID]?.Sampler;
                }

                Samplers.BindAll();
                Resources.BindAll();
                ConstantBuffers.BindAll();
            }
            else
            {
                // Do we unbind stage resources?
            }

            return shaderChanged;
        }

        internal abstract void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states);

        internal abstract void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views);

        internal abstract void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers);

        internal abstract void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances);

        internal CommandQueueDX11 Context { get; }

        internal ShaderType Type { get; }


        /// Gets the slots for binding <see cref="ShaderSamplerDX11"/> to the current <see cref="PipeShaderStage"/>.
        /// </summary>
        internal GraphicsSlotGroup<ShaderSamplerDX11> Samplers { get; }

        /// <summary>
        /// Gets the slots for binding <see cref="ContextBindableResource"/> to the current <see cref="PipeShaderStage"/>.
        /// </summary>
        internal GraphicsSlotGroup<ContextBindableResource> Resources { get; }

        /// <summary>
        /// Gets the slots for binding <see cref="ShaderConstantBuffer"/> to the current <see cref="PipeShaderStage"/>/
        /// </summary>
        internal GraphicsSlotGroup<ShaderConstantBuffer> ConstantBuffers { get; }

        /// <summary>
        /// Gets the shader bind slot for the current <see cref="PipeShaderStage{T, S}"/>
        /// </summary>
        internal GraphicsSlot<ShaderComposition<T>> Shader { get; }
    }
}

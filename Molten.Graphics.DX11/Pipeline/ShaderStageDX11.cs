using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe abstract class ShaderStageDX11
    {
        internal ShaderStageDX11(CommandQueueDX11 queue, ShaderType type)
        {
            Cmd = queue;
            Type = type;

            GraphicsCapabilities cap = Cmd.Device.Adapter.Capabilities;
            ShaderStageCapabilities shaderCap = cap[type];
            
            Samplers = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_Sampler", cap.MaxShaderSamplers, new SamplerGroupBinder(this));
            Resources = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_Resource", shaderCap.MaxInResources, new ResourceGroupBinder(this));
            ConstantBuffers = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_C-Buffer", cap.ConstantBuffers.MaxSlots, new CBufferGroupBinder(this));
            Shader = queue.RegisterSlot(GraphicsBindTypeFlags.Input, $"{type}_Shader", 0, new ShaderSlotBinder(this));
        }

        internal virtual bool Bind()
        {
            bool shaderChanged = Shader.Bind();

            ShaderComposition composition = Shader.BoundValue;

            if (composition != null)
            {
                // Apply pass constant buffers to slots
                for (int i = 0; i < composition.ConstBufferIds.Count; i++)
                {
                    uint slotID = composition.ConstBufferIds[i];
                    ConstantBuffers[slotID].Value = composition.Pass.Parent.ConstBuffers[slotID] as ShaderConstantBuffer;
                }

                // Apply pass resources to slots
                for (int i = 0; i < composition.ResourceIds.Count; i++)
                {
                    uint slotID = composition.ResourceIds[i];
                    Resources[slotID].Value = composition.Pass.Parent.Resources[slotID]?.Resource as GraphicsResourceDX11;
                }

                // Apply pass samplers to slots
                for (int i = 0; i < composition.SamplerIds.Count; i++)
                {
                    uint slotID = composition.SamplerIds[i];
                    Samplers[slotID].Value = composition.Pass.Parent.SamplerVariables[slotID]?.Sampler as ShaderSamplerDX11;
                }

                Samplers.BindAll();
                Resources.BindAll();
                ConstantBuffers.BindAll();
            }
            else
            {
                // NOTE Do we unbind stage resources?
            }

            return shaderChanged;
        }

        internal abstract void SetSamplers(uint startSlot, uint numSamplers, ID3D11SamplerState** states);

        internal abstract void SetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView1** views);

        internal abstract void SetConstantBuffers(uint startSlot, uint numBuffers, ID3D11Buffer** buffers);

        internal abstract void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances);

        internal CommandQueueDX11 Cmd { get; }

        internal ShaderType Type { get; }

        /// Gets the slots for binding <see cref="ShaderSamplerDX11"/> to the current <see cref="ContextShaderStage{T}"/>.
        /// </summary>
        internal GraphicsSlotGroup<ShaderSamplerDX11> Samplers { get; }

        /// <summary>
        /// Gets the slots for binding <see cref="GraphicsResourceDX11"/> to the current <see cref="ContextShaderStage{T}"/>.
        /// </summary>
        internal GraphicsSlotGroup<GraphicsResourceDX11> Resources { get; }

        /// <summary>
        /// Gets the slots for binding <see cref="ShaderConstantBuffer"/> to the current <see cref="ContextShaderStage{T}"/>/
        /// </summary>
        internal GraphicsSlotGroup<ShaderConstantBuffer> ConstantBuffers { get; }

        /// <summary>
        /// Gets the shader bind slot for the current <see cref="ContextShaderStage{T}"/>
        /// </summary>
        internal GraphicsSlot<ShaderComposition> Shader { get; }
    }
}

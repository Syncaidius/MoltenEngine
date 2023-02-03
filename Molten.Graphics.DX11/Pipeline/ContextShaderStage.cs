using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe abstract class ContextShaderStage<T>
        where T : unmanaged
    {
        internal ContextShaderStage(CommandQueueDX11 queue, ShaderType type)
        {
            Cmd = queue;
            Type = type;

            GraphicsCapabilities cap = Cmd.DXDevice.Adapter.Capabilities;
            ShaderStageCapabilities shaderCap = cap[type];
            
            Samplers = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_Sampler", cap.MaxShaderSamplers, new SamplerGroupBinder<T>(this));
            Resources = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_Resource", shaderCap.MaxInResources, new ResourceGroupBinder<T>(this));
            ConstantBuffers = queue.RegisterSlotGroup(GraphicsBindTypeFlags.Input, $"{type}_C-Buffer", cap.ConstantBuffers.MaxSlots, new CBufferGroupBinder<T>(this));
            Shader = queue.RegisterSlot(GraphicsBindTypeFlags.Input, $"{type}_Shader", 0, new ShaderSlotBinder<T>(this));
        }

        internal virtual bool Bind()
        {
            bool shaderChanged = Shader.Bind();

            ShaderCompositionDX11<T> composition = Shader.BoundValue;

            if (composition.PtrShader != null)
            {
                // Apply pass constant buffers to slots
                for (int i = 0; i < composition.ConstBufferIds.Count; i++)
                {
                    uint slotID = composition.ConstBufferIds[i];
                    ConstantBuffers[slotID].Value = composition.Parent.ConstBuffers[slotID] as ShaderConstantBuffer;
                }

                // Apply pass resources to slots
                for (int i = 0; i < composition.ResourceIds.Count; i++)
                {
                    uint slotID = composition.ResourceIds[i];
                    Resources[slotID].Value = composition.Parent.Resources[slotID]?.Resource as GraphicsResourceDX11;
                }

                // Apply pass samplers to slots
                for (int i = 0; i < composition.SamplerIds.Count; i++)
                {
                    uint slotID = composition.SamplerIds[i];
                    Samplers[slotID].Value = composition.Parent.SamplerVariables[slotID]?.Sampler as ShaderSamplerDX11;
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

        protected abstract void GetResources(uint startSlot, uint numViews, ID3D11ShaderResourceView** ptrViews);

        protected abstract void GetShader(void** shader, ID3D11ClassInstance** classInstances, uint* numClassInstances);

        internal void LogState()
        {
            Cmd.Device.Log.Debug($"   {Type} Shader State:");

            for (uint i = 0; i < Resources.SlotCount; i++)
            {
                if (Resources[i].Value == null)
                    continue;

                Cmd.Device.Log.Debug($"      Resource {i}/{Resources.SlotCount}: {Resources[i].Value}");
            }

            for (uint i = 0; i < Samplers.SlotCount; i++)
            {
                if (Samplers[i].Value == null)
                    continue;

                Cmd.Device.Log.Debug($"      Sampler {i}/{Samplers.SlotCount}: {Samplers[i].Value}");
            }

            for (uint i = 0; i < ConstantBuffers.SlotCount; i++)
            {
                if (ConstantBuffers[i].Value == null)
                    continue;

                Cmd.Device.Log.Debug($"      C-Buffer {i}/{ConstantBuffers.SlotCount}: {ConstantBuffers[i].Value}");
            }

            void* ptr = null;
            GetShader(&ptr, null, null);
            Cmd.Device.Log.Debug($"      Shader Ptr: {(nuint)ptr}");

            uint numViews = Resources.SlotCount;
            void* ptrSRVs = EngineUtil.Alloc((nuint)(sizeof(ID3D11ShaderResourceView*) * numViews));
            ID3D11ShaderResourceView** srvs = (ID3D11ShaderResourceView**)ptrSRVs;

            GetResources(0, numViews, srvs);
            for (uint i = 0; i < Resources.SlotCount; i++)
            {
                if (srvs[i] != null)
                    Cmd.Device.Log.Debug($"      SRV {i} Ptr: {(nuint)srvs[i]}");
            }

            EngineUtil.Free(ref ptrSRVs);
        }

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
        internal GraphicsSlot<ShaderCompositionDX11<T>> Shader { get; }
    }
}

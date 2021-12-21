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
    /// <typeparam name="T">The type of DX11 shader to be handled by this stage.</typeparam>
    internal unsafe abstract class PipeShaderStage<T> : PipeStage
        where T : unmanaged
    {
        internal PipeShaderStage(PipeDX11 pipe, ShaderType shaderType) : base(pipe)
        {
            ShaderStageType = shaderType;
            Name = $"Pipe{pipe.EOID} {shaderType} stage";

            uint maxSamplers = pipe.Device.Features.MaxSamplerSlots;
            Samplers = DefineSlotGroup<ShaderSampler>(maxSamplers, PipeBindTypeFlags.Input, "Sampler");

            uint maxResources = pipe.Device.Features.MaxInputResourceSlots;
            Resources = DefineSlotGroup<PipeBindableResource>(maxResources, PipeBindTypeFlags.Input, "Resource");

            uint maxCBuffers = pipe.Device.Features.MaxConstantBufferSlots;
            ConstantBuffers = DefineSlotGroup<ShaderConstantBuffer>(maxCBuffers, PipeBindTypeFlags.Input, "C-Buffer");

            Shader = DefineSlot<ShaderComposition<T>>(0, PipeBindTypeFlags.Input, "Shader");
        }

        internal bool Bind()
        {
            bool shaderChanged = Shader.Bind();

            if (shaderChanged)
            {
                ShaderComposition<T> composition = Shader.BoundValue;
                OnBindShader(Shader);

                if (composition != null)
                {
                    // Apply pass constant buffers to slots
                    for (int i = 0; i < composition.ConstBufferIds.Count; i++)
                    {
                        uint slotID = composition.ConstBufferIds[i];
                        ConstantBuffers[slotID].Value = composition.Parent.ConstBuffers[slotID];
                    }

                    // Apply pass resources to slots
                    for (int i = 0; i < composition.ConstBufferIds.Count; i++)
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
                }
                else
                {
                    // Do we unbind stage resources?
                }
            }


            bool cbChanged = ConstantBuffers.BindAll();
            bool resChanged = Resources.BindAll();
            bool samplersChanged = Samplers.BindAll();

            // Set constant buffers
            if (cbChanged)
            {
                int numChanged = (int)ConstantBuffers.NumSlotsChanged;
                ID3D11Buffer** cBuffers = stackalloc ID3D11Buffer*[numChanged];
                uint* cFirstConstants = stackalloc uint[numChanged];
                uint* cNumConstants = stackalloc uint[numChanged];

                uint sid = ConstantBuffers.FirstChanged;

                ShaderConstantBuffer cb = null;
                for (uint i = 0; i < numChanged; i++)
                {
                    cb = ConstantBuffers[sid].BoundValue;
                    if (cb != null)
                    {
                        cBuffers[i] = cb.ResourcePtr;
                        cFirstConstants[i] = 0; // TODO implement this using BufferSegment
                        cNumConstants[i] = (uint)cb.Variables.Length;
                    }
                    else
                    {
                        cBuffers[i] = null;
                        cFirstConstants[i] = 0;
                        cNumConstants[i] = 0;
                    }
                    sid++;
                }

                OnBindConstants(ConstantBuffers, cBuffers, cFirstConstants, cNumConstants);
            }

            // Set resources
            if (resChanged)
            {
                int numChanged = (int)Resources.NumSlotsChanged;
                ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[numChanged];

                uint sid = Resources.FirstChanged;
                for (int i = 0; i < numChanged; i++)
                    srvs[i] = Resources[sid].BoundValue ?? null;

                OnBindResources(Resources, srvs);
            }

            // Bind samplers
            if (samplersChanged)
            {
                int numChanged = (int)Samplers.NumSlotsChanged;
                ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[numChanged];

                uint sid = Samplers.FirstChanged;
                for (int i = 0; i < numChanged; i++)
                    samplers[i] = Samplers[sid].BoundValue ?? null;

                OnBindSamplers(Samplers, samplers);
            }

            return shaderChanged || cbChanged || resChanged || samplersChanged;
        }

        protected abstract void OnBindConstants(PipeSlotGroup<ShaderConstantBuffer> grp,
            ID3D11Buffer** buffers, uint* firsConstants, uint* numConstants);

        protected abstract void OnBindResources(PipeSlotGroup<PipeBindableResource> grp,
            ID3D11ShaderResourceView** resources);

        protected abstract void OnBindSamplers(PipeSlotGroup<ShaderSampler> grp,
            ID3D11SamplerState** resources);

        protected abstract void OnBindShader(PipeSlot<ShaderComposition<T>> slot);

        /// <summary>
        /// Gets the slots for binding <see cref="ShaderSampler"/> to the current <see cref="PipeShaderStage"/>.
        /// </summary>
        internal PipeSlotGroup<ShaderSampler> Samplers { get; }

        /// <summary>
        /// Gets the slots for binding <see cref="PipeBindableResource"/> to the current <see cref="PipeShaderStage"/>.
        /// </summary>
        internal PipeSlotGroup<PipeBindableResource> Resources { get; }

        /// <summary>
        /// Gets the slots for binding <see cref="ShaderConstantBuffer"/> to the current <see cref="PipeShaderStage"/>/
        /// </summary>
        internal PipeSlotGroup<ShaderConstantBuffer> ConstantBuffers { get; }

        /// <summary>
        /// Gets the shader bind slot for the current <see cref="PipeShaderStage{T, S}"/>
        /// </summary>
        internal PipeSlot<ShaderComposition<T>> Shader { get; }

        /// <summary>
        /// Gets the type of shader stage that the current <see cref="PipeShaderStage"/> represents.
        /// </summary>
        public ShaderType ShaderStageType { get; }
    }
}

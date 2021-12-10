﻿using Silk.NET.Direct3D11;
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

        internal override bool Bind()
        {
            // Set constant buffers
            if (ConstantBuffers.BindAll())
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
            if (Resources.BindAll())
            {
                int numChanged = (int)Resources.NumSlotsChanged;
                ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[numChanged];

                uint sid = Resources.FirstChanged;
                for (int i = 0; i < numChanged; i++)
                    srvs[i] = Resources[sid].BoundValue ?? null;

                OnBindResources(Resources, srvs);
            }

            // Bind samplers
            if (Samplers.BindAll())
            {
                int numChanged = (int)Samplers.NumSlotsChanged;
                ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[numChanged];

                uint sid = Samplers.FirstChanged;
                for (int i = 0; i < numChanged; i++)
                    samplers[i] = Samplers[sid].BoundValue ?? null;

                OnBindSamplers(Samplers, samplers);
            }

            if (Shader.Bind())
            {
                OnBindShader(Shader);
                return true;
            }

            return false;
        }

        protected abstract void OnBindConstants(PipeSlotGroup<ShaderConstantBuffer> grp,
            ID3D11Buffer** buffers, uint* firsConstants, uint* numConstants);

        protected abstract void OnBindResources(PipeSlotGroup<PipeBindableResource> grp,
            ID3D11ShaderResourceView** resources);

        protected abstract void OnBindSamplers(PipeSlotGroup<ShaderSampler> grp,
            ID3D11SamplerState** resources);

        protected abstract void OnBindShader(PipeSlot<HlslShader> slot);

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
        internal PipeSlot<HlslShader> Shader { get; }

        /// <summary>
        /// Gets the type of shader stage that the current <see cref="PipeShaderStage"/> represents.
        /// </summary>
        public ShaderType ShaderStageType { get; }
    }
}
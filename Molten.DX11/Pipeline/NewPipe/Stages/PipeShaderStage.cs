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

        internal override void Bind()
        {
            if (ConstantBuffers.BindAll())
            {
                int nChanged = (int)ConstantBuffers.NumSlotsChanged;
                ID3D11Buffer** cBuffers = stackalloc ID3D11Buffer*[nChanged];
                uint* cFirstConstants = stackalloc uint[nChanged];
                uint* cNumConstants = stackalloc uint[nChanged];

                uint sid = ConstantBuffers.FirstChanged;

                for (int i = 0; i < nChanged; i++)
                {
                    cBuffers[i] = ConstantBuffers[sid].BoundValue.Native;
                    cFirstConstants[i] = 0;
                    cNumConstants[i] = (uint)ConstantBuffers[sid].BoundValue.Variables.Length;
                }

                OnBindConstants(ConstantBuffers, cBuffers, cFirstConstants, cNumConstants);
            }

            // TODO Set Resources
            // TODO Set Samplers
            // TODO Set actual shader
        }
        protected abstract void OnBindConstants(PipeSlotGroup<ShaderConstantBuffer> grp,
            ID3D11Buffer** buffers, uint* firsConstants, uint* numConstants);

        /// <summary>
        /// Gets the slots for binding <see cref="ShaderSampler"/> to the current <see cref="PipeShaderStage"/>.
        /// </summary>
        internal PipeSlotGroup<PipeSampler> Samplers { get; }

        internal PipeSlotGroup<PipeBindableResource> Resources { get; }

        internal PipeSlotGroup<ShaderConstantBuffer> ConstantBuffers { get; }

        /// <summary>
        /// Gets the shader bind slot for the current <see cref="PipeShaderStage{T, S}"/>
        /// </summary>
        internal PipeBindSlot<HlslShader> Shader { get; }

        public ShaderType ShaderStageType { get; }
    }
}

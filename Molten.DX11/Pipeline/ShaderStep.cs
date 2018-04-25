using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ShaderStep<S, C, H>
        where S : DeviceChild
        where C: CommonShaderStage
        where H : HlslShader
    {
        internal PipelineBindSlot<PipelineShaderObject>[] _slotResources;
        internal PipelineBindSlot<ShaderConstantBuffer>[] _slotConstants;
        internal PipelineBindSlot<ShaderSampler>[] _slotSamplers;
        internal ShaderResourceView[] _resViews;

        C _stage;
        GraphicsPipe _pipe;
        H _boundShader;
        Action<C, ShaderComposition<S>> _setCallback;

        internal ShaderStep(GraphicsPipe pipe, ShaderInputStage<H> input, C shaderStage, Action<C, ShaderComposition<S>> setCallback)
        {
            // Setup slots
            GraphicsDeviceFeatures features = pipe.Device.Features;
            _stage = shaderStage;
            _pipe = pipe;
            _setCallback = setCallback;

            _slotResources = new PipelineBindSlot<PipelineShaderObject>[features.MaxInputResourceSlots];
            _resViews = new ShaderResourceView[_slotResources.Length];

            _slotConstants = new PipelineBindSlot<ShaderConstantBuffer>[features.MaxConstantBufferSlots];
            _slotSamplers = new PipelineBindSlot<ShaderSampler>[features.MaxInputSamplerSlots];

            for (int i = 0; i < features.MaxInputResourceSlots; i++)
            {
                _slotResources[i] = input.AddSlot<PipelineShaderObject>(PipelineSlotType.Input, i);
                _slotResources[i].OnBoundObjectDisposed += SlotResources_OnBoundObjectDisposed;
            }

            for (int i = 0; i < features.MaxConstantBufferSlots; i++)
            {
                _slotConstants[i] = input.AddSlot<ShaderConstantBuffer>(PipelineSlotType.Input, i);
                _slotConstants[i].OnBoundObjectDisposed += SlotConstants_OnBoundObjectDisposed;
            }

            for (int i = 0; i < features.MaxInputSamplerSlots; i++)
            {
                _slotSamplers[i] = input.AddSlot<ShaderSampler>(PipelineSlotType.Input, i);
                _slotSamplers[i].OnBoundObjectDisposed += EffectStageBase_OnBoundObjectDisposed;
            }
        }

        private void EffectStageBase_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            _stage.SetSampler(slot.SlotID, null);
        }

        private void SlotConstants_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            _stage.SetConstantBuffer(slot.SlotID, null);
        }

        private void SlotResources_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            _stage.SetShaderResource(slot.SlotID, null);
            _resViews[slot.SlotID] = null;
        }

        internal void Refresh(H shader, ShaderComposition<S> composition)
        {
            // Bind all constant buffers
            ShaderConstantBuffer cb = null;
            for (int i = 0; i < composition.ConstBufferIds.Count; i++)
            {
                int slotID = composition.ConstBufferIds[i];
                cb = shader.ConstBuffers[slotID];
                bool cbChanged = _slotConstants[slotID].Bind(_pipe, cb);

                if (cbChanged)
                    _stage.SetConstantBuffer(slotID, cb?.Buffer);
            }

            // Bind all resources
            ShaderResourceVariable variable = null;
            PipelineShaderObject resource = null;

            for (int i = 0; i < composition.ResourceIds.Count; i++)
            {
                int resID = composition.ResourceIds[i];

                variable = shader.Resources[resID];
                resource = variable?.Resource;

                bool resChanged = _slotResources[resID].Bind(_pipe, resource);

                if (resChanged)
                {
                    if (resource != null)
                    {
                        _resViews[resID] = resource.SRV;
                        _stage.SetShaderResource(_slotResources[resID].SlotID, resource.SRV);
                    }
                    else
                    {
                        _resViews[i] = null;
                        _stage.SetShaderResource(i, null);
                    }
                }
                else if (resource != null && _resViews[i] != resource.SRV)
                {
                    _resViews[i] = resource.SRV;
                    _stage.SetShaderResource(_slotResources[resID].SlotID, resource.SRV);
                }
            }

            // Bind all samplers
            ShaderSampler s = null;
            for (int i = 0; i < composition.SamplerIds.Count; i++)
            {
                int slotId = composition.SamplerIds[i];

                s = shader.SamplerVariables[slotId].Sampler;

                bool sChanged = _slotSamplers[i].Bind(_pipe, s);
                if (sChanged)
                    _stage.SetSampler(i, s?.State);
            }

            if (_boundShader != shader)
            {
                _boundShader = shader;
                _setCallback(_stage, composition);
                _pipe.Profiler.CurrentFrame.ShaderSwaps++;
            }
        }

        /// <summary>Gets the underlying DX11 <see cref="CommonShaderStage"/> instance.</summary>
        internal C RawStage => _stage;
    }
}

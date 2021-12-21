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
        internal PipelineBindSlot<PipelineShaderObject, DeviceDX11, PipeDX11>[] _slotResources;
        internal PipelineBindSlot<ShaderConstantBuffer, DeviceDX11, PipeDX11>[] _slotConstants;
        internal PipelineBindSlot<ShaderSampler, DeviceDX11, PipeDX11>[] _slotSamplers;
        internal ShaderResourceView[] _resViews;

        C _stage;
        PipeDX11 _pipe;
        S _boundShader;
        Action<C, ShaderComposition<S>> _setCallback;

        internal ShaderStep(PipeDX11 pipe, ShaderInputStage<H> input, C shaderStage, Action<C, ShaderComposition<S>> setCallback)
        {
            // Setup slots
            DeviceFeaturesDX11 features = pipe.Device.Features;
            _stage = shaderStage;
            _pipe = pipe;
            _setCallback = setCallback;

            _slotResources = new PipelineBindSlot<PipelineShaderObject, DeviceDX11, PipeDX11>[features.MaxInputResourceSlots];
            _resViews = new ShaderResourceView[_slotResources.Length];

            _slotConstants = new PipelineBindSlot<ShaderConstantBuffer, DeviceDX11, PipeDX11>[features.MaxConstantBufferSlots];
            _slotSamplers = new PipelineBindSlot<ShaderSampler, DeviceDX11, PipeDX11>[features.MaxSamplerSlots];

            for (int i = 0; i < features.MaxInputResourceSlots; i++)
            {
                _slotResources[i] = input.AddSlot<PipelineShaderObject>(i);
                _slotResources[i].OnObjectForcedUnbind += SlotResources_OnBoundObjectDisposed;
            }

            for (int i = 0; i < features.MaxConstantBufferSlots; i++)
            {
                _slotConstants[i] = input.AddSlot<ShaderConstantBuffer>(i);
                _slotConstants[i].OnObjectForcedUnbind += SlotConstants_OnBoundObjectDisposed;
            }

            for (int i = 0; i < features.MaxSamplerSlots; i++)
            {
                _slotSamplers[i] = input.AddSlot<ShaderSampler>(i);
                _slotSamplers[i].OnObjectForcedUnbind += EffectStageBase_OnBoundObjectDisposed;
            }
        }

        private void EffectStageBase_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, PipelineDisposableObject obj)
        {
            _stage.SetSampler(slot.SlotID, null);
        }

        private void SlotConstants_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, PipelineDisposableObject obj)
        {
            _stage.SetConstantBuffer(slot.SlotID, null);
        }

        private void SlotResources_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, PipelineDisposableObject obj)
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
                bool cbChanged = _slotConstants[slotID].Bind(_pipe, cb, PipelineBindType.Input);

                if (cbChanged)
                    _stage.SetConstantBuffer(slotID, cb?.Buffer);
            }

            // Bind all resources
            ShaderResourceVariable variable = null;
            PipelineShaderObject resource = null;

            for (int i = 0; i < composition.ResourceIds.Count; i++)
            {
                int resID = composition.ResourceIds[i];
                PipelineBindSlot<PipelineShaderObject, DeviceDX11, PipeDX11> slot = _slotResources[resID];
                int slotID = slot.SlotID;

                variable = shader.Resources[resID];
                resource = variable?.Resource;


                bool resChanged = slot.Bind(_pipe, resource, PipelineBindType.Input);

                if (resChanged)
                {
                    if (resource != null)
                    {
                        _resViews[slotID] = resource.SRV;
                        _stage.SetShaderResource(slotID, resource.SRV);

                    }
                    else
                    {
                        _resViews[slotID] = null;
                        _stage.SetShaderResource(slotID, null);
                    }
                }
                else
                {
                    if (resource != null)
                    {
                        if (_resViews[slotID] != resource.SRV)
                        {
                            _resViews[slotID] = resource.SRV;
                            _stage.SetShaderResource(slotID, resource.SRV);
                        }
                    }
                    else if (_resViews[slotID] != null)
                    {
                        _resViews[slotID] = null;
                        _stage.SetShaderResource(slotID, null);
                    }
                }
            }

            // Bind all samplers
            ShaderSampler sampler = null;
            for (int i = 0; i < composition.SamplerIds.Count; i++)
            {
                int slotId = composition.SamplerIds[i];
                sampler = shader.SamplerVariables[slotId].Sampler;

                bool sChanged = _slotSamplers[slotId].Bind(_pipe, sampler, PipelineBindType.Input);
                if (sChanged)
                    _stage.SetSampler(slotId, sampler?.State);
            }

            if (_boundShader != composition.RawShader)
            {
                _boundShader = composition.RawShader;
                _setCallback(_stage, composition);
                _pipe.Profiler.Current.ShaderSwaps++;
            }
        }

        /// <summary>Gets the underlying DX11 <see cref="CommonShaderStage"/> instance.</summary>
        internal C RawStage => _stage;
    }
}

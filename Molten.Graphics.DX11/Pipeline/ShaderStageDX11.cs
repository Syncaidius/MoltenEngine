using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    internal unsafe abstract class ShaderStageDX11
    {
        GraphicsStateValueGroup<ConstantBufferDX11> _constantBuffers;
        GraphicsStateValueGroup<GraphicsResource> _resources;
        GraphicsStateValueGroup<ShaderSamplerDX11> _samplers;
        GraphicsStateValue<ShaderComposition> _shader;

        internal ShaderStageDX11(GraphicsQueueDX11 queue, ShaderType type)
        {
            Cmd = queue;
            Type = type;

            GraphicsCapabilities cap = Cmd.Device.Capabilities;
            ShaderStageCapabilities shaderCap = cap[type];

            _samplers = new GraphicsStateValueGroup<ShaderSamplerDX11>(cap.MaxShaderSamplers);
            _resources = new GraphicsStateValueGroup<GraphicsResource>(shaderCap.MaxInResources);
            _constantBuffers = new GraphicsStateValueGroup<ConstantBufferDX11>(cap.ConstantBuffers.MaxSlots);
            _shader = new GraphicsStateValue<ShaderComposition>();
        }

        internal bool Bind(ShaderComposition c)
        {
            _shader.Value = c;
            bool shaderChanged = _shader.Bind(Cmd);
            c = _shader.BoundValue;

            if (shaderChanged)
            {
                if (c != null)
                    SetShader(c.PtrShader, null, 0);
                else
                    SetShader(null, null, 0);
            }

            // Clear current bindings
            _samplers.Reset();
            _resources.Reset();
            _constantBuffers.Reset();

            if (c != null)
            {
                // Apply pass samplers to slots
                for (int i = 0; i < c.SamplerIds.Count; i++)
                {
                    uint slotID = c.SamplerIds[i];
                    _samplers[slotID] = c.Pass.Parent.SamplerVariables[slotID]?.Sampler as ShaderSamplerDX11;
                }

                // Apply pass resources to slots
                for (int i = 0; i < c.ResourceIds.Count; i++)
                {
                    uint slotID = c.ResourceIds[i];
                    _resources[slotID] = c.Pass.Parent.Resources[slotID]?.Resource;
                }

                // Apply pass constant buffers to slots
                for (int i = 0; i < c.ConstBufferIds.Count; i++)
                {
                    uint slotID = c.ConstBufferIds[i];
                    _constantBuffers[slotID] = c.Pass.Parent.ConstBuffers[slotID] as ConstantBufferDX11;
                }
            }

            BindSamplers();
            BindResources();
            BindConstantBuffers();

            OnBind(c, shaderChanged);

            return shaderChanged;
        }

        protected virtual void OnBind(ShaderComposition c, bool shaderChanged) { }

        private void BindSamplers()
        {
            if (!_samplers.Bind(Cmd))
                return;

            ID3D11SamplerState** samplers = stackalloc ID3D11SamplerState*[_samplers.Length];

            for (int i = 0; i < _samplers.Length; i++)
            {
                if (_samplers.BoundValues[i] != null)
                    samplers[i] = _samplers.BoundValues[i].NativePtr;
                else
                    samplers[i] = null;
            }

            SetSamplers((uint)_samplers.Length, samplers);
        }

        private void BindResources()
        {
            if(!_resources.Bind(Cmd))
                return;

            ID3D11ShaderResourceView1** res = stackalloc ID3D11ShaderResourceView1*[_resources.Length];
            for (int i = 0; i < _resources.Length; i++)
            {
                if (_resources.BoundValues[i] != null)
                    res[i] = (ID3D11ShaderResourceView1*)_resources.BoundValues[i].SRV;
                else
                    res[i] = null;
            }

            SetResources((uint)_resources.Length, res);
        }

        private void BindConstantBuffers()
        {
            if (!_constantBuffers.Bind(Cmd))
                return;

            int count = _constantBuffers.Length;
            ID3D11Buffer** cBuffers = stackalloc ID3D11Buffer*[count];
            uint* cFirstConstants = stackalloc uint[count];
            uint* cNumConstants = stackalloc uint[count];
            ConstantBufferDX11 cb = null;

            for (int i = 0; i < count; i++)
            {
                cb = _constantBuffers.BoundValues[i];

                if (cb != null)
                {
                    cBuffers[i] = (ID3D11Buffer*)cb.Handle;
                    cFirstConstants[i] = 0; // TODO implement this using BufferSegment
                    cNumConstants[i] = (uint)cb.Variables.Length;
                }
                else
                {
                    cBuffers[i] = null;
                    cFirstConstants[i] = 0;
                    cNumConstants[i] = 0;
                }
            }

            SetConstantBuffers((uint)count, cBuffers);
        }

        internal abstract void SetSamplers(uint numSamplers, ID3D11SamplerState** states);

        internal abstract void SetResources(uint numViews, ID3D11ShaderResourceView1** views);

        internal abstract void SetConstantBuffers(uint numBuffers, ID3D11Buffer** buffers);

        internal abstract void SetShader(void* shader, ID3D11ClassInstance** classInstances, uint numClassInstances);

        internal GraphicsQueueDX11 Cmd { get; }

        internal ShaderType Type { get; }
    }
}

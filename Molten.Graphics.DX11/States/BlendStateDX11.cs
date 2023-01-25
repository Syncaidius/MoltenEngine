using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a blend state for use with a <see cref="CommandQueueDX11"/>.</summary>
    public unsafe class BlendStateDX11 : GraphicsBlendState
    {
        public static readonly BlendDesc1 _defaultDesc;

        /// <summary>
        /// Gets or sets the blend sample mask.
        /// </summary>
        public uint BlendSampleMask { get; set; }

        /// <summary>
        /// Gets or sets the blend factor.
        /// </summary>
        public Color4 BlendFactor { get; set; }

        static BlendStateDX11()
        {
            _defaultDesc = new BlendDesc1()
            {
                AlphaToCoverageEnable = 0,
                IndependentBlendEnable = 0,
            };

            _defaultDesc.RenderTarget[0] = new RenderTargetBlendDesc1()
            {
                SrcBlend = Blend.One,
                DestBlend = Blend.Zero,
                BlendOp = BlendOp.Add,
                SrcBlendAlpha = Blend.One,
                DestBlendAlpha = Blend.Zero,
                BlendOpAlpha = BlendOp.Add,
                RenderTargetWriteMask = (byte)ColorWriteEnable.All,
                BlendEnable = 1,
                LogicOp = LogicOp.Noop,
                LogicOpEnable = 0,
            };
        }

        public unsafe ID3D11BlendState1* NativePtr => _native;

        ID3D11BlendState1* _native;
        BlendDesc1 _desc;

        bool _dirty;

        protected override RenderSurfaceBlend InitializeSurfaceBlend(int index, RenderSurfaceBlend source)
        {
            throw new NotImplementedException();
        }

        public BlendStateDX11(DeviceDX11 device, BlendStateDX11 source) : base(device, source)
        {
            _desc = source._desc;
            BlendFactor = source.BlendFactor;
            BlendSampleMask = source.BlendSampleMask;
        }

        internal BlendStateDX11(DeviceDX11 device) : 
            base(device)
        {
            _desc = _defaultDesc;
            BlendFactor = new Color4(1, 1, 1, 1);
            BlendSampleMask = 0xffffffff;
        }

        internal BlendStateDX11(DeviceDX11 device, RenderTargetBlendDesc1 rtDesc) : 
            base(device)
        {
            _desc = _defaultDesc;
            _desc.RenderTarget[0] = rtDesc;
            BlendFactor = new Color4(1, 1, 1, 1);
            BlendSampleMask = 0xffffffff;
        }

        internal RenderTargetBlendDesc1 GetSurfaceBlendState(int index)
        {
            return _desc.RenderTarget[index];
        }

        public bool Equals(BlendStateDX11 other)
        {
            if (_desc.IndependentBlendEnable != other._desc.IndependentBlendEnable)
                return false;

            if (_desc.AlphaToCoverageEnable != other._desc.AlphaToCoverageEnable)
                return false;

            // Equality check against all RT blend states
            for(int i = 0; i < Device.Adapter.Capabilities.PixelShader.MaxOutResources; i++)
            {
                RenderTargetBlendDesc1 rt = _desc.RenderTarget[i];
                RenderTargetBlendDesc1 otherRt = other._desc.RenderTarget[i];

                if (rt.BlendOpAlpha != otherRt.BlendOpAlpha ||
                    rt.BlendOp != otherRt.BlendOp ||
                    rt.DestBlendAlpha != otherRt.DestBlendAlpha ||
                    rt.DestBlend != otherRt.DestBlend ||
                    rt.BlendEnable != otherRt.BlendEnable ||
                    rt.RenderTargetWriteMask != otherRt.RenderTargetWriteMask ||
                    rt.SrcBlendAlpha != otherRt.SrcBlendAlpha ||
                    rt.SrcBlend != otherRt.SrcBlend)
                {
                    return false;
                }
            }
            return true;
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (_native == null || _dirty)
            {
                _dirty = false;
                SilkUtil.ReleasePtr(ref _native);

                // Create new state
                (cmd as CommandQueueDX11).DXDevice.Ptr->CreateBlendState1(ref _desc, ref _native);
            }
        }

        public static implicit operator ID3D11BlendState1*(BlendStateDX11 state)
        {
            return state._native;
        }

        public static implicit operator ID3D11BlendState*(BlendStateDX11 state)
        {
            return (ID3D11BlendState*)state._native;
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public override bool AlphaToCoverageEnable
        {
            get => _desc.AlphaToCoverageEnable > 0;
            set
            {
                _desc.AlphaToCoverageEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public override bool IndependentBlendEnable
        {
            get => _desc.IndependentBlendEnable > 0;
            set
            {
                _desc.IndependentBlendEnable = value ? 1 : 0;
                _dirty = true;
            }
        }
    }
}

using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a blend state for use with a <see cref="DeviceContext"/>.</summary>
    internal unsafe class GraphicsBlendState : ContextBindable<ID3D11BlendState>, IEquatable<GraphicsBlendState>
    {
        public static readonly BlendDesc _defaultDesc;

        /// <summary>
        /// Gets or sets the blend sample mask.
        /// </summary>
        public uint BlendSampleMask { get; set; }

        /// <summary>
        /// Gets or sets the blend factor.
        /// </summary>
        public Color4 BlendFactor { get; set; }

        static GraphicsBlendState()
        {
            _defaultDesc = new BlendDesc()
            {
                AlphaToCoverageEnable = 0,
                IndependentBlendEnable = 0,
            };

            _defaultDesc.RenderTarget[0] = new RenderTargetBlendDesc()
            {
                SrcBlend = Blend.One,
                DestBlend = Blend.Zero,
                BlendOp = BlendOp.Add,
                SrcBlendAlpha = Blend.One,
                DestBlendAlpha = Blend.Zero,
                BlendOpAlpha = BlendOp.Add,
                RenderTargetWriteMask = (byte)ColorWriteEnable.All,
                BlendEnable = 1
            };
        }

        internal override unsafe ID3D11BlendState* NativePtr => _native;

        ID3D11BlendState* _native;
        BlendDesc _desc;

        bool _dirty;

        internal GraphicsBlendState(DeviceDX11 device, GraphicsBlendState source) : base(device, ContextBindTypeFlags.Input)
        {
            _desc = source._desc;
            BlendFactor = source.BlendFactor;
            BlendSampleMask = source.BlendSampleMask;
        }

        internal GraphicsBlendState(DeviceDX11 device) : base(device, ContextBindTypeFlags.Input)
        {
            _desc = _defaultDesc;
            BlendFactor = new Color4(1, 1, 1, 1);
            BlendSampleMask = 0xffffffff;
        }

        internal GraphicsBlendState(DeviceDX11 device, RenderTargetBlendDesc rtDesc) : base(device, ContextBindTypeFlags.Input)
        {
            _desc = _defaultDesc;
            _desc.RenderTarget[0] = rtDesc;
            BlendFactor = new Color4(1, 1, 1, 1);
            BlendSampleMask = 0xffffffff;
        }

        internal RenderTargetBlendDesc GetSurfaceBlendState(int index)
        {
            return _desc.RenderTarget[index];
        }

        public override bool Equals(object obj)
        {
            if (obj is GraphicsBlendState other)
                return Equals(other);
            else
                return false;
        }

        public bool Equals(GraphicsBlendState other)
        {
            if (_desc.IndependentBlendEnable != other._desc.IndependentBlendEnable)
                return false;

            if (_desc.AlphaToCoverageEnable != other._desc.AlphaToCoverageEnable)
                return false;

            // Equality check against all RT blend states
            for(int i = 0; i < Device.Features.SimultaneousRenderSurfaces; i++)
            {
                RenderTargetBlendDesc rt = _desc.RenderTarget[i];
                RenderTargetBlendDesc otherRt = other._desc.RenderTarget[i];

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

        protected override void OnApply(DeviceContext pipe)
        {
            if (_native == null || _dirty)
            {
                _dirty = false;
                SilkUtil.ReleasePtr(ref _native);

                // Create new state
                Device.NativeDevice->CreateBlendState(ref _desc, ref _native);
            }
        }

        public static implicit operator ID3D11BlendState*(GraphicsBlendState state)
        {
            return state._native;
        }

        internal override void PipelineRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public bool AlphaToCoverageEnable
        {
            get => _desc.AlphaToCoverageEnable > 0;
            set
            {
                _desc.AlphaToCoverageEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public bool IndependentBlendEnable
        {
            get => _desc.IndependentBlendEnable > 0;
            set
            {
                _desc.IndependentBlendEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a render target blend description at the specified index.
        /// </summary>
        /// <param name="rtIndex">The render target/surface blend index.</param>
        /// <returns></returns>
        internal RenderTargetBlendDesc this[int rtIndex]
        {
            get => _desc.RenderTarget[rtIndex];
            set
            {
                _desc.RenderTarget[rtIndex] = value;
                _dirty = true;
            }
        }
    }
}

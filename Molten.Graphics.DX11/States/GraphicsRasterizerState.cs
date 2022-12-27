using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a rasterizer state for use with a <see cref="DeviceContext"/>.</summary>
    internal unsafe class GraphicsRasterizerState : ContextBindable<ID3D11RasterizerState>
    {
        static RasterizerDesc _defaultDesc;

        internal override unsafe ID3D11RasterizerState* NativePtr => _native;

        ID3D11RasterizerState* _native;
        RasterizerDesc _desc;
        bool _dirty;

        static GraphicsRasterizerState()
        {
            _defaultDesc = new RasterizerDesc()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                FrontCounterClockwise = 0,
                DepthBias = 0,
                SlopeScaledDepthBias = 0.0f,
                DepthBiasClamp = 0.0f,
                DepthClipEnable = 1,
                ScissorEnable = 0,
                MultisampleEnable = 0,
                AntialiasedLineEnable = 0
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">An existing <see cref="GraphicsRasterizerState"/> instance from which to copy settings."/></param>
        internal GraphicsRasterizerState(Device device, GraphicsRasterizerState source) : base(device, ContextBindTypeFlags.Input)
        {
            _desc = source._desc;
            _dirty = true;
        }

        internal GraphicsRasterizerState(Device device) : base(device, ContextBindTypeFlags.Input)
        {
            _desc = _defaultDesc;
            _dirty = true;
        }

        public override bool Equals(object obj)
        {
            if (obj is GraphicsRasterizerState other)
                return Equals(other);
            else
                return false;
        }

        public bool Equals(GraphicsRasterizerState other)
        {
            return _desc.CullMode == other._desc.CullMode &&
                _desc.DepthBias == other._desc.DepthBias &&
                _desc.DepthBiasClamp == other._desc.DepthBiasClamp &&
                _desc.FillMode == other._desc.FillMode &&
                _desc.AntialiasedLineEnable == other._desc.AntialiasedLineEnable &&
                _desc.DepthClipEnable == other._desc.DepthClipEnable &&
                _desc.FrontCounterClockwise == other._desc.FrontCounterClockwise &&
                _desc.MultisampleEnable == other._desc.MultisampleEnable &&
                _desc.ScissorEnable == other._desc.ScissorEnable &&
                _desc.SlopeScaledDepthBias == other._desc.SlopeScaledDepthBias;
        }

        protected override void OnApply(DeviceContext pipe)
        {
            if (_native == null || _dirty)
            {
                _dirty = false;
                SilkUtil.ReleasePtr(ref _native);

                //create new state
                Device.NativeDevice->CreateRasterizerState(ref _desc, ref _native);
            }
        }

        internal override void PipelineRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public static implicit operator ID3D11RasterizerState* (GraphicsRasterizerState state)
        {
            return state._native;
        }

        public CullMode CullMode
        {
            get { return _desc.CullMode; }
            set
            {
                _desc.CullMode = value;
                _dirty = true;
            }
        }

        public int DepthBias
        {
            get { return _desc.DepthBias; }
            set
            {
                _desc.DepthBias = value;
                _dirty = true;
            }
        }

        public float DepthBiasClamp
        {
            get { return _desc.DepthBiasClamp; }
            set
            {
                _desc.DepthBiasClamp = value;
                _dirty = true;
            }
        }

        public FillMode FillMode
        {
            get { return _desc.FillMode; }
            set
            {
                _desc.FillMode = value;
                _dirty = true;
            }
        }

        public bool IsAntialiasedLineEnabled
        {
            get { return _desc.AntialiasedLineEnable > 0; }
            set
            {
                _desc.AntialiasedLineEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public bool IsDepthClipEnabled
        {
            get { return _desc.DepthClipEnable > 0; }
            set
            {
                _desc.DepthClipEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public bool IsFrontCounterClockwise
        {
            get { return _desc.FrontCounterClockwise > 0; }
            set
            {
                _desc.FrontCounterClockwise = value ? 1 : 0;
                _dirty = true;
            }
        }

        public bool IsMultisampleEnabled
        {
            get { return _desc.MultisampleEnable > 0; }
            set
            {
                _desc.MultisampleEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public bool IsScissorEnabled
        {
            get { return _desc.ScissorEnable > 0; }
            set
            {
                _desc.ScissorEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public float SlopeScaledDepthBias
        {
            get { return _desc.SlopeScaledDepthBias; }
            set
            {
                _desc.SlopeScaledDepthBias = value;
                _dirty = true;
            }
        }
    }
}

using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="DeviceContext"/>.</summary>
    internal unsafe class GraphicsDepthState : PipeBindable<ID3D11DepthStencilState>, IEquatable<GraphicsDepthState>
    {
        public class Face
        {
            internal DepthStencilopDesc _desc;
            GraphicsDepthState _parent;

            internal Face(GraphicsDepthState parent, ref DepthStencilopDesc defaultDesc)
            {
                _parent = parent;
                _desc = defaultDesc;
            }

            public ComparisonFunc Comparison
            {
                get { return _desc.StencilFunc; }
                set
                {
                    _desc.StencilFunc = value;
                    _parent._dirty = true;
                }
            }

            public StencilOp PassOperation
            {
                get { return _desc.StencilPassOp; }
                set
                {
                    _desc.StencilPassOp = value;
                    _parent._dirty = true;
                }
            }

            public StencilOp FailOperation
            {
                get { return _desc.StencilFailOp; }
                set
                {
                    _desc.StencilFailOp = value;
                    _parent._dirty = true;
                }
            }

            public StencilOp DepthFailOperation
            {
                get { return _desc.StencilDepthFailOp; }
                set
                {
                    _desc.StencilDepthFailOp = value;
                    _parent._dirty = true;
                }
            }
        }

        static DepthStencilDesc _defaultDesc;

        internal override unsafe ID3D11DepthStencilState* NativePtr => _native;
        ID3D11DepthStencilState* _native;
        DepthStencilDesc _desc;
        internal bool _dirty;

        Face _frontFace;
        Face _backFace;

        static GraphicsDepthState()
        {
            _defaultDesc = new DepthStencilDesc()
            {
                DepthEnable = 1,
                DepthWriteMask = DepthWriteMask.DepthWriteMaskAll,
                DepthFunc = ComparisonFunc.ComparisonLess,
                StencilEnable = 0,
                StencilReadMask = D3D11.DefaultStencilReadMask,
                StencilWriteMask = D3D11.DefaultStencilWriteMask,
                FrontFace = new DepthStencilopDesc()
                {
                    StencilFunc = ComparisonFunc.ComparisonAlways,
                    StencilDepthFailOp = StencilOp.StencilOpKeep,
                    StencilPassOp = StencilOp.StencilOpKeep,
                    StencilFailOp = StencilOp.StencilOpKeep,
                },
                BackFace = new DepthStencilopDesc()
                {
                    StencilFunc = ComparisonFunc.ComparisonAlways,
                    StencilDepthFailOp = StencilOp.StencilOpKeep,
                    StencilPassOp = StencilOp.StencilOpKeep,
                    StencilFailOp = StencilOp.StencilOpKeep,
                }
            };
        }

        internal GraphicsDepthState(Device device, GraphicsDepthState source) : base(device, ContextBindTypeFlags.Input)
        {
            _desc = source._desc;
            _frontFace = new Face(this, ref _desc.FrontFace);
            _backFace = new Face(this, ref _desc.BackFace);
        }

        internal GraphicsDepthState(Device device) : base(device, ContextBindTypeFlags.Input)
        {
            _desc = _defaultDesc;
            _frontFace = new Face(this, ref _desc.FrontFace);
            _backFace = new Face(this, ref _desc.BackFace);
        }

        public override bool Equals(object obj)
        {
            if (obj is GraphicsDepthState other)
                return Equals(other);
            else
                return false;
        }

        public bool Equals(GraphicsDepthState other)
        {
            if (!CompareOperation(ref _desc.BackFace, ref other._desc.BackFace) || !CompareOperation(ref _desc.FrontFace, ref other._desc.FrontFace))
                return false;

            return _desc.DepthFunc == other._desc.DepthFunc &&
                _desc.DepthEnable == other._desc.DepthEnable &&
                _desc.StencilEnable == other._desc.StencilEnable &&
                _desc.StencilReadMask == other._desc.StencilReadMask &&
                _desc.StencilWriteMask == other._desc.StencilWriteMask;
        }

        private static bool CompareOperation(ref DepthStencilopDesc op, ref DepthStencilopDesc other)
        {
            return op.StencilFunc == other.StencilFunc &&
                op.StencilDepthFailOp == other.StencilDepthFailOp &&
                op.StencilFailOp == other.StencilFailOp &&
                op.StencilPassOp == other.StencilPassOp;
        }

        public void SetFrontFace(DepthStencilopDesc desc)
        {
            _frontFace._desc = desc;
            _dirty = true;
        }

        public void SetBackFace(DepthStencilopDesc desc)
        {
            _backFace._desc = desc;
            _dirty = true;
        }

        protected override void OnApply(DeviceContext pipe)
        {
            if (_native == null || _dirty)
            {
                _dirty = false;
                SilkUtil.ReleasePtr(ref _native);

                //copy the front and back-face settings into the main description
                _desc.FrontFace = _frontFace._desc;
                _desc.BackFace = _backFace._desc;

                //create new state
                Device.NativeDevice->CreateDepthStencilState(ref _desc, ref _native);
            }
        }

        internal override void PipelineRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        internal bool IsDepthEnabled
        {
            get { return _desc.DepthEnable > 0; }
            set
            {
                _desc.DepthEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        internal bool IsStencilEnabled
        {
            get { return _desc.StencilEnable > 0; }
            set
            {
                _desc.StencilEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        internal DepthWriteMask DepthWriteMask
        {
            get { return _desc.DepthWriteMask; }
            set
            {
                _desc.DepthWriteMask = value;
                _dirty = true;
            }
        }

        internal ComparisonFunc DepthComparison
        {
            get { return _desc.DepthFunc; }
            set
            {
                _desc.DepthFunc = value;
                _dirty = true;
            }
        }

        internal byte StencilReadMask
        {
            get { return _desc.StencilReadMask; }
            set
            {
                _desc.StencilReadMask = value;
                _dirty = true;
            }
        }

        internal byte StencilWriteMask
        {
            get { return _desc.StencilWriteMask; }
            set
            {
                _desc.StencilWriteMask = value;
                _dirty = true;
            }
        }

        /// <summary>Gets the description for the front-face depth operation description.</summary>
        internal Face FrontFace
        {
            get { return _frontFace; }
        }

        /// <summary>Gets the description for the back-face depth operation description.</summary>
        internal Face BackFace
        {
            get { return _backFace; }
        }

        /// <summary>Gets or sets the stencil reference value. The default value is 0.</summary>
        public uint StencilReference { get; set; }

        /// <summary>
        /// Gets or sets the depth write permission. the default value is <see cref="GraphicsDepthWritePermission.Enabled"/>.
        /// </summary>
        public GraphicsDepthWritePermission WritePermission { get; set; } = GraphicsDepthWritePermission.Enabled;
    }
}

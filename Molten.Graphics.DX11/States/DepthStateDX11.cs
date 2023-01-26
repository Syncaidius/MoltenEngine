using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="CommandQueueDX11"/>.</summary>
    internal unsafe class DepthStateDX11 : GraphicsDepthState
    {
        public class FaceDX11 : Face
        {
            internal DepthStencilopDesc _desc;
            DepthStateDX11 _parent;

            internal FaceDX11(DepthStateDX11 parent, ref DepthStencilopDesc defaultDesc)
            {
                _parent = parent;
                _desc = defaultDesc;
            }

            public override ComparisonFunction Comparison
            {
                get { return (ComparisonFunction)_desc.StencilFunc; }
                set
                {
                    _desc.StencilFunc = (ComparisonFunc)value;
                    _parent._dirty = true;
                }
            }

            public override DepthStencilOperation PassOperation
            {
                get { return (DepthStencilOperation)_desc.StencilPassOp; }
                set
                {
                    _desc.StencilPassOp = (StencilOp)value;
                    _parent._dirty = true;
                }
            }

            public override DepthStencilOperation FailOperation
            {
                get { return (DepthStencilOperation)_desc.StencilFailOp; }
                set
                {
                    _desc.StencilFailOp = (StencilOp)value;
                    _parent._dirty = true;
                }
            }

            public override DepthStencilOperation DepthFailOperation
            {
                get { return (DepthStencilOperation)_desc.StencilDepthFailOp; }
                set
                {
                    _desc.StencilDepthFailOp = (StencilOp)value;
                    _parent._dirty = true;
                }
            }
        }

        static DepthStencilDesc _defaultDesc;
        
        public unsafe ID3D11DepthStencilState* NativePtr => _native;
        ID3D11DepthStencilState* _native;
        DepthStencilDesc _desc;
        internal bool _dirty;

        FaceDX11 _frontFace;
        FaceDX11 _backFace;

        internal DepthStateDX11(DeviceDX11 device, DepthStateDX11 source = null) :
            base(device, source)
        { }

        protected override Face CreateFace(bool isFrontFace)
        {
            if(isFrontFace)
                return new FaceDX11(this, ref _desc.FrontFace);
            else
                return new FaceDX11(this, ref _desc.BackFace);
        }

        public override bool Equals(object obj)
        {
            if (obj is DepthStateDX11 other)
                return Equals(other);
            else
                return false;
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (_native == null || _dirty)
            {
                _dirty = false;
                SilkUtil.ReleasePtr(ref _native);

                //copy the front and back-face settings into the main description
                _desc.FrontFace = _frontFace._desc;
                _desc.BackFace = _backFace._desc;

                //create new state
                (cmd as CommandQueueDX11).DXDevice.Ptr->CreateDepthStencilState(ref _desc, ref _native);
            }
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public override bool IsDepthEnabled
        {
            get { return _desc.DepthEnable > 0; }
            set
            {
                _desc.DepthEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public override bool IsStencilEnabled
        {
            get { return _desc.StencilEnable > 0; }
            set
            {
                _desc.StencilEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public override DepthWriteFlags WriteFlags
        {
            get { return (DepthWriteFlags)_desc.DepthWriteMask; }
            set
            {
                _desc.DepthWriteMask = (DepthWriteMask)value;
                _dirty = true;
            }
        }

        public override ComparisonFunction DepthComparison
        {
            get { return (ComparisonFunction)_desc.DepthFunc; }
            set
            {
                _desc.DepthFunc = (ComparisonFunc)value;
                _dirty = true;
            }
        }

        public override byte StencilReadMask
        {
            get { return _desc.StencilReadMask; }
            set
            {
                _desc.StencilReadMask = value;
                _dirty = true;
            }
        }

        public override byte StencilWriteMask
        {
            get { return _desc.StencilWriteMask; }
            set
            {
                _desc.StencilWriteMask = value;
                _dirty = true;
            }
        }

        /// <summary>Gets the description for the front-face depth operation description.</summary>
        public override Face FrontFace => _frontFace;

        /// <summary>Gets the description for the back-face depth operation description.</summary>
        public override Face BackFace => _backFace;

        public static implicit operator ID3D11DepthStencilState*(DepthStateDX11 bindable)
        {
            return bindable.NativePtr;
        }
    }
}

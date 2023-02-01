using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a rasterizer state for use with a <see cref="CommandQueueDX11"/>.</summary>
    internal unsafe class RasterizerStateDX11 : GraphicsRasterizerState
    {
        public unsafe ID3D11RasterizerState2* NativePtr => _native;

        ID3D11RasterizerState2* _native;
        RasterizerDesc2 _desc;
        bool _dirty;

        /// <summary>
        /// Creates a new instance of <see cref="RasterizerStateDX11"/>.
        /// </summary>
        /// <param name="source">An existing <see cref="RasterizerStateDX11"/> instance from which to copy settings."/></param>
        internal RasterizerStateDX11(DeviceDX11 device, RasterizerStateDX11 source = null) : 
            base(device, source)
        {
            _dirty = true;
        }

        public override bool Equals(object obj)
        {
            if (obj is RasterizerStateDX11 other)
                return Equals(other);
            else
                return false;
        }

        public bool Equals(RasterizerStateDX11 other)
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
                _desc.SlopeScaledDepthBias == other._desc.SlopeScaledDepthBias &&
                _desc.ConservativeRaster == other._desc.ConservativeRaster &&
                _desc.ForcedSampleCount == other._desc.ForcedSampleCount;
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (_native == null || _dirty)
            {
                _dirty = false;
                SilkUtil.ReleasePtr(ref _native);

                //create new state
                (cmd as CommandQueueDX11).DXDevice.Ptr->CreateRasterizerState2(ref _desc, ref _native);
                Version++;
            }
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public static implicit operator ID3D11RasterizerState2* (RasterizerStateDX11 state)
        {
            return state._native;
        }

        public static implicit operator ID3D11RasterizerState*(RasterizerStateDX11 state)
        {
            return (ID3D11RasterizerState*)state._native;
        }

        public override RasterizerCullingMode CullingMode
        {
            get { return (RasterizerCullingMode)_desc.CullMode; }
            set
            {
                if (_desc.CullMode != (CullMode)value)
                {
                    _desc.CullMode = (CullMode)value;
                    _dirty = true;
                }
            }
        }

        public override int DepthBias
        {
            get => _desc.DepthBias;
            set
            {
                if (_desc.DepthBias != value)
                {
                    _desc.DepthBias = value;
                    _dirty = true;
                }

            }
        }

        public override float DepthBiasClamp
        {
            get => _desc.DepthBiasClamp;
            set
            {
                if (_desc.DepthBiasClamp != value)
                {
                    _desc.DepthBiasClamp = value;
                    _dirty = true;
                }
            }
        }

        public override RasterizerFillingMode FillingMode
        {
            get => (RasterizerFillingMode)_desc.FillMode;
            set
            {
                if (_desc.FillMode != (FillMode)value)
                {
                    _desc.FillMode = (FillMode)value;
                    _dirty = true;
                }
            }
        }

        public override bool IsAntialiasedLineEnabled
        {
            get => _desc.AntialiasedLineEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_desc.AntialiasedLineEnable != val)
                {
                    _desc.AntialiasedLineEnable = val;
                    _dirty = true;
                }
            }
        }

        public override bool IsDepthClipEnabled
        {
            get => _desc.DepthClipEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_desc.DepthClipEnable != val)
                {
                    _desc.DepthClipEnable = val;
                    _dirty = true;
                }
            }
        }

        public override bool IsFrontCounterClockwise
        {
            get => _desc.FrontCounterClockwise > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_desc.FrontCounterClockwise != val)
                {
                    _desc.FrontCounterClockwise = val;
                    _dirty = true;
                }
            }
        }

        public override bool IsMultisampleEnabled
        {
            get => _desc.MultisampleEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_desc.MultisampleEnable != val)
                {
                    _desc.MultisampleEnable = val;
                    _dirty = true;
                }
            }
        }

        public override bool IsScissorEnabled
        {
            get => _desc.ScissorEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_desc.ScissorEnable != val)
                {
                    _desc.ScissorEnable = val;
                    _dirty = true;
                }
            }
        }

        public override float SlopeScaledDepthBias
        {
            get => _desc.SlopeScaledDepthBias;
            set
            {
                if (_desc.SlopeScaledDepthBias != value)
                {
                    _desc.SlopeScaledDepthBias = value;
                    _dirty = true;
                }
            }
        }

        public override ConservativeRasterizerMode ConservativeRaster
        {
            get => (ConservativeRasterizerMode)_desc.ConservativeRaster;
            set
            {
                if (_desc.ConservativeRaster != (ConservativeRasterizationMode)value)
                {
                    _desc.ConservativeRaster = (ConservativeRasterizationMode)value;
                    _dirty = true;
                }
            }
        }

        public override uint ForcedSampleCount
        {
            get => _desc.ForcedSampleCount;
            set
            {
                if (_desc.ForcedSampleCount != value)
                {
                    _desc.ForcedSampleCount = value;
                    _dirty = true;
                }
            }
        }
    }
}

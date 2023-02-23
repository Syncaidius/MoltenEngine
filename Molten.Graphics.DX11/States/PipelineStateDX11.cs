using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class PipelineStateDX11 : GraphicsPipelineState
    {
        public class FaceDX11 : Face
        {
            internal DepthStencilopDesc _desc;
            PipelineStateDX11 _parent;

            internal FaceDX11(PipelineStateDX11 parent, ref DepthStencilopDesc defaultDesc)
            {
                _parent = parent;
                _desc = defaultDesc;
            }

            public override ComparisonFunction Comparison
            {
                get => (ComparisonFunction)_desc.StencilFunc;
                set
                {
                    if (_desc.StencilFunc != (ComparisonFunc)value)
                    {
                        _desc.StencilFunc = (ComparisonFunc)value;
                        _parent._dirtyDepth = true;
                    }
                }
            }

            public override DepthStencilOperation StencilPass
            {
                get => (DepthStencilOperation)_desc.StencilPassOp;
                set
                {
                    if (_desc.StencilPassOp != (StencilOp)value)
                    {
                        _desc.StencilPassOp = (StencilOp)value;
                        _parent._dirtyDepth = true;
                    }
                }
            }

            public override DepthStencilOperation StencilFail
            {
                get => (DepthStencilOperation)_desc.StencilFailOp;
                set
                {
                    if (_desc.StencilFailOp != (StencilOp)value)
                    {
                        _desc.StencilFailOp = (StencilOp)value;
                        _parent._dirtyDepth = true;
                    }
                }
            }

            public override DepthStencilOperation DepthFail
            {
                get => (DepthStencilOperation)_desc.StencilDepthFailOp;
                set
                {
                    if (_desc.StencilDepthFailOp != (StencilOp)value)
                    {
                        _desc.StencilDepthFailOp = (StencilOp)value;
                        _parent._dirtyDepth = true;
                    }
                }
            }
        }

        public class SurfaceBlendDX11 : RenderSurfaceBlend
        {
            PipelineStateDX11 _parent = null;
            int _index;

            internal SurfaceBlendDX11(PipelineStateDX11 parent, int index)
            {
                _parent = parent;
                _index = index;
            }

            public override bool BlendEnable
            {
                get => _parent._descBlend.Value.RenderTarget[_index].BlendEnable == 1;
                set
                {
                    int val = value ? 1 : 0;
                    if (_parent._descBlend.Value.RenderTarget[_index].BlendEnable != val)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].BlendEnable = val;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override bool LogicOpEnable
            {
                get => _parent._descBlend.Value.RenderTarget[_index].LogicOpEnable > 0;
                set
                {
                    int val = value ? 1 : 0;
                    if (_parent._descBlend.Value.RenderTarget[_index].LogicOpEnable != val)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].LogicOpEnable = val;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override BlendType SrcBlend
            {
                get => (BlendType)_parent._descBlend.Value.RenderTarget[_index].SrcBlend;
                set
                {
                    if (_parent._descBlend.Value.RenderTarget[_index].SrcBlend != (Blend)value)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].SrcBlend = (Blend)value;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override BlendType DestBlend
            {
                get => (BlendType)_parent._descBlend.Value.RenderTarget[_index].DestBlend;
                set
                {
                    if (_parent._descBlend.Value.RenderTarget[_index].DestBlend != (Blend)value)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].DestBlend = (Blend)value;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override BlendOperation BlendOp
            {
                get => (BlendOperation)_parent._descBlend.Value.RenderTarget[_index].BlendOp;
                set
                {
                    if (_parent._descBlend.Value.RenderTarget[_index].BlendOp != (BlendOp)value)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].BlendOp = (BlendOp)value;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override BlendType SrcBlendAlpha
            {
                get => (BlendType)_parent._descBlend.Value.RenderTarget[_index].SrcBlendAlpha;
                set
                {
                    if (_parent._descBlend.Value.RenderTarget[_index].SrcBlendAlpha != (Blend)value)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].SrcBlendAlpha = (Blend)value;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override BlendType DestBlendAlpha
            {
                get => (BlendType)_parent._descBlend.Value.RenderTarget[_index].DestBlendAlpha;
                set
                {
                    if (_parent._descBlend.Value.RenderTarget[_index].DestBlendAlpha != (Blend)value)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].DestBlendAlpha = (Blend)value;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override BlendOperation BlendOpAlpha
            {
                get => (BlendOperation)_parent._descBlend.Value.RenderTarget[_index].BlendOpAlpha;
                set
                {
                    if (_parent._descBlend.Value.RenderTarget[_index].BlendOpAlpha != (BlendOp)value)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].BlendOpAlpha = (BlendOp)value;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override LogicOperation LogicOp
            {
                get => (LogicOperation)_parent._descBlend.Value.RenderTarget[_index].LogicOp;
                set
                {
                    if (_parent._descBlend.Value.RenderTarget[_index].LogicOp != (LogicOp)value)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].LogicOp = (LogicOp)value;
                        _parent._dirtyBlend = true;
                    }
                }
            }

            public override ColorWriteFlags RenderTargetWriteMask
            {
                get => (ColorWriteFlags)_parent._descBlend.Value.RenderTarget[_index].RenderTargetWriteMask;
                set
                {
                    if (_parent._descBlend.Value.RenderTarget[_index].RenderTargetWriteMask != (byte)value)
                    {
                        _parent._descBlend.Value.RenderTarget[_index].RenderTargetWriteMask = (byte)value;
                        _parent._dirtyBlend = true;
                    }
                }
            }
        }

        StructKey<DepthStencilDesc> _descDepth;
        bool _dirtyDepth = true;

        StructKey<RasterizerDesc2> _descRaster;
        bool _dirtyRaster = true;

        StructKey<BlendDesc1> _descBlend;
        bool _dirtyBlend = true;

        internal PipelineStateDX11(DeviceDX11 device, PipelineStatePreset preset) :
            base(device)
        {
            device.StatePresets.ApplyPreset(this, preset);
        }

        protected override void Initialize()
        {
            _descDepth = new StructKey<DepthStencilDesc>(); // TODO get default
            DepthState = new DepthStateDX11(Device, _descDepth);

            _descRaster = new StructKey<RasterizerDesc2>();
            RasterizerState = new RasterizerStateDX11(Device, _descRaster);

            _descBlend = new StructKey<BlendDesc1>();
            BlendState = new BlendStateDX11(Device, _descBlend, Color4.White, 0);
        }

        public override void GraphicsRelease()
        {
            _descDepth.Dispose();
            _descRaster.Dispose();
            _descBlend.Dispose();
        }

        protected override Face CreateFace(bool isFrontFace)
        {
            if (isFrontFace)
                return new FaceDX11(this, ref _descDepth.Value.FrontFace);
            else
                return new FaceDX11(this, ref _descDepth.Value.BackFace);
        }

        protected override RenderSurfaceBlend CreateSurfaceBlend(int index)
        {
            return new SurfaceBlendDX11(this, index);
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            DeviceDX11 device = Device as DeviceDX11;

            if (_dirtyDepth)
            {
                _descDepth.Value.FrontFace = (DepthFrontFace as FaceDX11)._desc;
                _descDepth.Value.BackFace = (DepthBackFace as FaceDX11)._desc;

                DepthState = device.CacheObject(_descDepth, new DepthStateDX11(Device, _descDepth));

                _dirtyDepth = false;
                Version++;
            }

            if (_dirtyRaster)
            {
                RasterizerState = device.CacheObject(_descRaster, new RasterizerStateDX11(Device, _descRaster));
                _dirtyRaster = false;
                Version++;
            }

            if (_dirtyBlend)
            {
                BlendState = device.CacheObject(_descBlend, new BlendStateDX11(Device, _descBlend, BlendFactor, BlendSampleMask));
                _dirtyBlend = false;
                Version++;
            }
        }

        internal DepthStateDX11 DepthState { get; private set; }

        internal RasterizerStateDX11 RasterizerState { get; private set; }

        internal BlendStateDX11 BlendState { get; private set; }

        #region Depth-Stencil Properties
        public override bool IsDepthEnabled
        {
            get => _descDepth.Value.DepthEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_descDepth.Value.DepthEnable != val)
                {
                    _descDepth.Value.DepthEnable = val;
                    _dirtyDepth = true;
                }
            }
        }

        public override bool IsStencilEnabled
        {
            get => _descDepth.Value.StencilEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_descDepth.Value.StencilEnable != val)
                {
                    _descDepth.Value.StencilEnable = val;
                    _dirtyDepth = true;
                }
            }
        }

        public override bool DepthWriteEnabled
        {
            get => _descDepth.Value.DepthWriteMask == DepthWriteMask.All;
            set
            {
                DepthWriteMask mask = value ? DepthWriteMask.All : DepthWriteMask.None;
                if (_descDepth.Value.DepthWriteMask != mask)
                {
                    _descDepth.Value.DepthWriteMask = mask;
                    _dirtyDepth = true;
                }
            }
        }

        public override ComparisonFunction DepthComparison
        {
            get => (ComparisonFunction)_descDepth.Value.DepthFunc;
            set
            {
                if (_descDepth.Value.DepthFunc != (ComparisonFunc)value)
                {
                    _descDepth.Value.DepthFunc = (ComparisonFunc)value;
                    _dirtyDepth = true;
                }
            }
        }

        public override byte StencilReadMask
        {
            get => _descDepth.Value.StencilReadMask;
            set
            {
                if (_descDepth.Value.StencilReadMask != value)
                {
                    _descDepth.Value.StencilReadMask = value;
                    _dirtyDepth = true;
                }
            }
        }

        public override byte StencilWriteMask
        {
            get => _descDepth.Value.StencilWriteMask;
            set
            {
                if (_descDepth.Value.StencilWriteMask != value)
                {
                    _descDepth.Value.StencilWriteMask = value;
                    _dirtyDepth = true;
                }
            }
        }

        public override bool DepthBoundsTestEnabled
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override float MaxDepthBounds { get; set; }

        public override float MinDepthBounds { get; set; }
        #endregion

        #region Rasterizer Properties
        public override RasterizerCullingMode Cull
        {
            get { return (RasterizerCullingMode)_descRaster.Value.CullMode; }
            set
            {
                if (_descRaster.Value.CullMode != (CullMode)value)
                {
                    _descRaster.Value.CullMode = (CullMode)value;
                    _dirtyRaster = true;
                }
            }
        }

        public override int DepthBias
        {
            get => _descRaster.Value.DepthBias;
            set
            {
                if (_descRaster.Value.DepthBias != value)
                {
                    _descRaster.Value.DepthBias = value;
                    _dirtyRaster = true;
                }

            }
        }

        public override float DepthBiasClamp
        {
            get => _descRaster.Value.DepthBiasClamp;
            set
            {
                if (_descRaster.Value.DepthBiasClamp != value)
                {
                    _descRaster.Value.DepthBiasClamp = value;
                    _dirtyRaster = true;
                }
            }
        }

        public override RasterizerFillingMode Fill
        {
            get => (RasterizerFillingMode)_descRaster.Value.FillMode;
            set
            {
                if (_descRaster.Value.FillMode != (FillMode)value)
                {
                    _descRaster.Value.FillMode = (FillMode)value;
                    _dirtyRaster = true;
                }
            }
        }

        public override bool IsAALineEnabled
        {
            get => _descRaster.Value.AntialiasedLineEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_descRaster.Value.AntialiasedLineEnable != val)
                {
                    _descRaster.Value.AntialiasedLineEnable = val;
                    _dirtyRaster = true;
                }
            }
        }

        public override bool IsDepthClipEnabled
        {
            get => _descRaster.Value.DepthClipEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_descRaster.Value.DepthClipEnable != val)
                {
                    _descRaster.Value.DepthClipEnable = val;
                    _dirtyRaster = true;
                }
            }
        }

        public override bool IsFrontCounterClockwise
        {
            get => _descRaster.Value.FrontCounterClockwise > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_descRaster.Value.FrontCounterClockwise != val)
                {
                    _descRaster.Value.FrontCounterClockwise = val;
                    _dirtyRaster = true;
                }
            }
        }

        public override bool IsMultisampleEnabled
        {
            get => _descRaster.Value.MultisampleEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_descRaster.Value.MultisampleEnable != val)
                {
                    _descRaster.Value.MultisampleEnable = val;
                    _dirtyRaster = true;
                }
            }
        }

        public override bool IsScissorEnabled
        {
            get => _descRaster.Value.ScissorEnable > 0;
            set
            {
                int val = value ? 1 : 0;
                if (_descRaster.Value.ScissorEnable != val)
                {
                    _descRaster.Value.ScissorEnable = val;
                    _dirtyRaster = true;
                }
            }
        }

        public override float SlopeScaledDepthBias
        {
            get => _descRaster.Value.SlopeScaledDepthBias;
            set
            {
                if (_descRaster.Value.SlopeScaledDepthBias != value)
                {
                    _descRaster.Value.SlopeScaledDepthBias = value;
                    _dirtyRaster = true;
                }
            }
        }

        public override ConservativeRasterizerMode ConservativeRaster
        {
            get => (ConservativeRasterizerMode)_descRaster.Value.ConservativeRaster;
            set
            {
                if (_descRaster.Value.ConservativeRaster != (ConservativeRasterizationMode)value)
                {
                    _descRaster.Value.ConservativeRaster = (ConservativeRasterizationMode)value;
                    _dirtyRaster = true;
                }
            }
        }

        public override uint ForcedSampleCount
        {
            get => _descRaster.Value.ForcedSampleCount;
            set
            {
                if (_descRaster.Value.ForcedSampleCount != value)
                {
                    _descRaster.Value.ForcedSampleCount = value;
                    _dirtyRaster = true;
                }
            }
        }
        #endregion

        #region Blend Properties
        public override bool AlphaToCoverageEnable
        {
            get => _descBlend.Value.AlphaToCoverageEnable > 0;
            set
            {
                _descBlend.Value.AlphaToCoverageEnable = value ? 1 : 0;
                _dirtyBlend = true;
            }
        }

        public override bool IndependentBlendEnable
        {
            get => _descBlend.Value.IndependentBlendEnable > 0;
            set
            {
                _descBlend.Value.IndependentBlendEnable = value ? 1 : 0;
                _dirtyBlend = true;
            }
        }

        public override Color4 BlendFactor
        {
            get => BlendState.BlendFactor;
            set => BlendState.BlendFactor = value;
        }

        public override uint BlendSampleMask
        {
            get => BlendState.BlendSampleMask;
            set => BlendState.BlendSampleMask = value;
        }
        #endregion
    }
}

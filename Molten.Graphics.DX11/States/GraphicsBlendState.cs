using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a blend state for use with a <see cref="DeviceContext"/>.</summary>
    internal unsafe class GraphicsBlendState : PipeBindable<ID3D11BlendState1>, IEquatable<GraphicsBlendState>
    {
        static BlendDesc1 _defaultDesc;

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
            _defaultDesc = new BlendDesc1()
            {
                AlphaToCoverageEnable = 0,
                IndependentBlendEnable = 0,
            };

            _defaultDesc.RenderTarget[0] = new RenderTargetBlendDesc1()
            {
                SrcBlend = Blend.BlendOne,
                DestBlend = Blend.BlendZero,
                BlendOp = BlendOp.BlendOpAdd,
                SrcBlendAlpha = Blend.BlendOne,
                DestBlendAlpha = Blend.BlendZero,
                BlendOpAlpha = BlendOp.BlendOpAdd,
                RenderTargetWriteMask = (byte)ColorWriteEnable.ColorWriteEnableAll,
                BlendEnable = 1,
                LogicOpEnable = 0,
                LogicOp = LogicOp.LogicOpNoop
            };
        }

        internal override unsafe ID3D11BlendState1* NativePtr => _native;

        ID3D11BlendState1* _native;
        BlendDesc1 _desc;

        bool _dirty;

        internal GraphicsBlendState(Device device, GraphicsBlendState source) : base(device)
        {
            _desc = source._desc;
            BlendFactor = source.BlendFactor;
            BlendSampleMask = source.BlendSampleMask;
        }

        internal GraphicsBlendState(Device device) : base(device)
        {
            _desc = _defaultDesc;
            BlendFactor = new Color4(1, 1, 1, 1);
            BlendSampleMask = 0xffffffff;
        }

        internal GraphicsBlendState(Device device, RenderTargetBlendDesc1 rtDesc) : base(device)
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

        protected internal override void Refresh(PipeSlot slot, DeviceContext pipe)
        {
            if (_native == null || _dirty)
            {
                _dirty = false;
                SilkUtil.ReleasePtr(ref _native);

                // Create new state
                Device.NativeDevice->CreateBlendState1(ref _desc, ref _native);
            }
        }

        public static implicit operator ID3D11BlendState*(GraphicsBlendState state)
        {
            return (ID3D11BlendState*)state._native;
        }

        internal override void PipelineDispose()
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
        internal RenderTargetBlendDesc1 this[int rtIndex]
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

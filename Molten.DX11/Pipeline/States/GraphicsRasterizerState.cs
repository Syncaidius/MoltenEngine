using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Stores a rasterizer state for use with a <see cref="PipeDX11"/>.</summary>
    internal unsafe class GraphicsRasterizerState : PipelineObject<DeviceDX11, PipeDX11>
    {
        static RasterizerDesc1 _defaultDesc;

        internal ID3D11RasterizerState1* Native;
        RasterizerDesc1 _desc;
        bool _dirty;

        static GraphicsRasterizerState()
        {
            _defaultDesc = new RasterizerDesc1()
            {
                FillMode = FillMode.FillSolid,
                CullMode = CullMode.CullBack,
                FrontCounterClockwise = 0,
                DepthBias = 0,
                SlopeScaledDepthBias = 0.0f,
                DepthBiasClamp = 0.0f,
                DepthClipEnable = 1,
                ScissorEnable = 0,
                MultisampleEnable = 0,
                AntialiasedLineEnable = 0,
                ForcedSampleCount = 0,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">An existing <see cref="GraphicsRasterizerState"/> instance from which to copy settings."/></param>
        internal GraphicsRasterizerState(DeviceDX11 device, GraphicsRasterizerState source) : base(device)
        {
            _desc = source._desc;
            _dirty = true;
        }

        internal GraphicsRasterizerState(DeviceDX11 device) : base(device)
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

        internal override void Refresh(PipeDX11 pipe, PipelineBindSlot<DeviceDX11, PipeDX11> slot)
        {
            if (Native == null || _dirty)
            {
                _dirty = false;

                //dispose of previous state object
                if (Native != null)
                {
                    Native->Release();
                    Native = null;
                }

                //create new state
                Device.Native->CreateRasterizerState1(ref _desc, ref Native);
            }
        }

        private protected override void OnPipelineDispose()
        {
            if(Native != null)
            {
                Native->Release();
                Native = null;
            }
        }

        public static implicit operator ID3D11RasterizerState* (GraphicsRasterizerState state)
        {
            return (ID3D11RasterizerState*)state.Native;
        }

        public static implicit operator ID3D11RasterizerState1*(GraphicsRasterizerState state)
        {
            return state.Native;
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

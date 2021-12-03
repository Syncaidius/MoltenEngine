using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Stores a rasterizer state for use with a <see cref="PipeDX11"/>.</summary>
    internal class GraphicsRasterizerState : PipelineObject<DeviceDX11, PipeDX11>
    {
        internal ID3D11RasterizerState Native;
        RasterizerDesc1 _desc;
        bool _dirty;

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
            _desc = RasterizerStateDescription.Default();
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
                _desc.IsAntialiasedLineEnabled == other._desc.IsAntialiasedLineEnabled &&
                _desc.IsDepthClipEnabled == other._desc.IsDepthClipEnabled &&
                _desc.IsFrontCounterClockwise == other._desc.IsFrontCounterClockwise &&
                _desc.IsMultisampleEnabled == other._desc.IsMultisampleEnabled &&
                _desc.IsScissorEnabled == other._desc.IsScissorEnabled &&
                _desc.SlopeScaledDepthBias == other._desc.SlopeScaledDepthBias;
        }

        internal override void Refresh(PipeDX11 pipe, PipelineBindSlot<DeviceDX11, PipeDX11> slot)
        {
            if (Native == null || _dirty)
            {
                _dirty = false;

                //dispose of previous state object
                if (Native != null)
                    Native.Dispose();

                //create new state
                Native = new RasterizerState(pipe.Device.D3d, _desc);
            }
        }

        private protected override void OnPipelineDispose()
        {
            DisposeObject(ref Native);
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
            get { return _desc.IsAntialiasedLineEnabled; }
            set
            {
                _desc.IsAntialiasedLineEnabled = value;
                _dirty = true;
            }
        }

        public bool IsDepthClipEnabled
        {
            get { return _desc.IsDepthClipEnabled; }
            set
            {
                _desc.IsDepthClipEnabled = value;
                _dirty = true;
            }
        }

        public bool IsFrontCounterClockwise
        {
            get { return _desc.IsFrontCounterClockwise; }
            set
            {
                _desc.IsFrontCounterClockwise = value;
                _dirty = true;
            }
        }

        public bool IsMultisampleEnabled
        {
            get { return _desc.IsMultisampleEnabled; }
            set
            {
                _desc.IsMultisampleEnabled = value;
                _dirty = true;
            }
        }

        public bool IsScissorEnabled
        {
            get { return _desc.IsScissorEnabled; }
            set
            {
                _desc.IsScissorEnabled = value;
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

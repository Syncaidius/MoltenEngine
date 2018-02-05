using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Stores a rasterizer state for use with a <see cref="GraphicsPipe"/>.</summary>
    internal class GraphicsRasterizerState : PipelineObject
    {
        internal RasterizerState State;
        RasterizerStateDescription _desc;
        bool _dirty;

        public GraphicsRasterizerState()
        {
            _desc = RasterizerStateDescription.Default();
            _dirty = true;
        }

        internal override void Refresh(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            if (State == null || _dirty)
            {
                _dirty = false;

                //dispose of previous state object
                if (State != null)
                    State.Dispose();

                //create new state
                State = new RasterizerState(pipe.Device.D3d, _desc);
            }
        }

        protected override void OnDispose()
        {
            DisposeObject(ref State);

            base.OnDispose();
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

using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="GraphicsPipe"/>.</summary>
    internal class GraphicsDepthState : PipelineObject
    {
        public class Face
        {
            internal DepthStencilOperationDescription _desc;
            GraphicsDepthState _parent;

            internal Face(GraphicsDepthState parent, DepthStencilOperationDescription defaultDesc)
            {
                _parent = parent;
                _desc = defaultDesc;
            }

            public Comparison Comparison
            {
                get { return _desc.Comparison; }
                set
                {
                    _desc.Comparison = value;
                    _parent._dirty = true;
                }
            }

            public StencilOperation PassOperation
            {
                get { return _desc.PassOperation; }
                set
                {
                    _desc.PassOperation = value;
                    _parent._dirty = true;
                }
            }

            public StencilOperation FailOperation
            {
                get { return _desc.FailOperation; }
                set
                {
                    _desc.FailOperation = value;
                    _parent._dirty = true;
                }
            }

            public StencilOperation DepthFailOperation
            {
                get { return _desc.DepthFailOperation; }
                set
                {
                    _desc.DepthFailOperation = value;
                    _parent._dirty = true;
                }
            }
        }

        internal DepthStencilState State;
        DepthStencilStateDescription _desc;
        internal bool _dirty;

        Face _frontFace;
        Face _backFace;

        internal GraphicsDepthState()
        {
            _desc = DepthStencilStateDescription.Default();
            _frontFace = new Face(this, _desc.FrontFace);
            _backFace = new Face(this, _desc.BackFace);
        }

        public void SetFrontFace(DepthStencilOperationDescription desc)
        {
            _frontFace._desc = desc;
            _dirty = true;
        }

        public void SetBackFace(DepthStencilOperationDescription desc)
        {
            _backFace._desc = desc;
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

                //copy the front and back-face settings into the main description
                _desc.FrontFace = _frontFace._desc;
                _desc.BackFace = _backFace._desc;

                //create new state
                State = new DepthStencilState(pipe.Device.D3d, _desc);
            }
        }

        protected override void OnDispose()
        {
            DisposeObject(ref State);

            base.OnDispose();
        }

        public bool IsDepthEnabled
        {
            get { return _desc.IsDepthEnabled; }
            set
            {
                _desc.IsDepthEnabled = value;
                _dirty = true;
            }
        }

        public bool IsStencilEnabled
        {
            get { return _desc.IsStencilEnabled; }
            set
            {
                _desc.IsStencilEnabled = value;
                _dirty = true;
            }
        }

        public DepthWriteMask DepthWriteMask
        {
            get { return _desc.DepthWriteMask; }
            set
            {
                _desc.DepthWriteMask = value;
                _dirty = true;
            }
        }

        public Comparison DepthComparison
        {
            get { return _desc.DepthComparison; }
            set
            {
                _desc.DepthComparison = value;
                _dirty = true;
            }
        }

        public byte StencilReadMask
        {
            get { return _desc.StencilReadMask; }
            set
            {
                _desc.StencilReadMask = value;
                _dirty = true;
            }
        }

        public byte StencilWriteMask
        {
            get { return _desc.StencilWriteMask; }
            set
            {
                _desc.StencilWriteMask = value;
                _dirty = true;
            }
        }

        /// <summary>Gets the description for the front-face depth operation description.</summary>
        public Face FrontFace
        {
            get { return _frontFace; }
        }

        /// <summary>Gets the description for the back-face depth operation description.</summary>
        public Face BackFace
        {
            get { return _backFace; }
        }
    }
}

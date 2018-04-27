using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Stores a depth-stencil state for use with a <see cref="GraphicsPipe"/>.</summary>
    internal class GraphicsDepthState : PipelineObject, IEquatable<GraphicsDepthState>
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

        internal GraphicsDepthState(GraphicsDepthState source)
        {
            _desc = source._desc;
            _frontFace = new Face(this, _desc.FrontFace);
            _backFace = new Face(this, _desc.BackFace);
        }

        internal GraphicsDepthState()
        {
            _desc = DepthStencilStateDescription.Default();
            _frontFace = new Face(this, _desc.FrontFace);
            _backFace = new Face(this, _desc.BackFace);
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

            return _desc.DepthComparison == other._desc.DepthComparison &&
                _desc.IsDepthEnabled == other._desc.IsDepthEnabled &&
                _desc.IsStencilEnabled == other._desc.IsStencilEnabled &&
                _desc.StencilReadMask == other._desc.StencilReadMask &&
                _desc.StencilWriteMask == other._desc.StencilWriteMask;
        }

        private static bool CompareOperation(ref DepthStencilOperationDescription op, ref DepthStencilOperationDescription other)
        {
            return op.Comparison == other.Comparison &&
                op.DepthFailOperation == other.DepthFailOperation &&
                op.FailOperation == other.FailOperation &&
                op.PassOperation == other.PassOperation;
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

        internal bool IsDepthEnabled
        {
            get { return _desc.IsDepthEnabled; }
            set
            {
                _desc.IsDepthEnabled = value;
                _dirty = true;
            }
        }

        internal bool IsStencilEnabled
        {
            get { return _desc.IsStencilEnabled; }
            set
            {
                _desc.IsStencilEnabled = value;
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

        internal Comparison DepthComparison
        {
            get { return _desc.DepthComparison; }
            set
            {
                _desc.DepthComparison = value;
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
        public int StencilReference { get; set; }
    }
}

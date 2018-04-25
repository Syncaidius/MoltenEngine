using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class GraphicsDepthStage : PipelineComponent
    {
        PipelineBindSlot<GraphicsDepthState> _slotState;
        GraphicsDepthState _currentState = null;
        int _stencilRef = 0;
        bool _stencilRefChanged = false;

        internal GraphicsDepthStage(GraphicsPipe pipe) : base(pipe)
        {
            _slotState = AddSlot<GraphicsDepthState>(PipelineSlotType.Output, 0);
            _slotState.OnBoundObjectDisposed += _slotState_OnBoundObjectDisposed;
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            Pipe.Context.OutputMerger.DepthStencilState = null;
        }

        protected override void OnDispose()
        {
            _currentState = null;
            base.OnDispose();
        }

        public void SetPreset(DepthStencilPreset preset)
        {
            _currentState = Device.GetPreset(preset);
        }

        /// <summary>Applies the current state to the device. Called internally.</summary>
        internal void Refresh()
        {
            // Ensure the default preset is used if a null state was requested.
            _currentState = _currentState ?? Device.GetPreset(DepthStencilPreset.Default);
            bool stateChanged = _slotState.Bind(Pipe, _currentState);

            if (stateChanged)
            {
                Pipe.Context.OutputMerger.SetDepthStencilState(_slotState.BoundObject.State, _stencilRef);
                _stencilRefChanged = false;
            }
            else if (_stencilRefChanged)
            {
                Pipe.Context.OutputMerger.DepthStencilReference = _stencilRef;
                _stencilRefChanged = false;
            }
        }

        /// <summary>Gets or sets the current graphics depth-stencil state.</summary>
        public GraphicsDepthState Current
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        /// <summary>Gets or sets the stencil reference value. The default value is 0.</summary>
        public int StencilReference
        {
            get { return _stencilRef; }
            set
            {
                if (_stencilRef != value)
                {
                    _stencilRef = value;
                    _stencilRefChanged = true;
                }
            }
        }
    }
}

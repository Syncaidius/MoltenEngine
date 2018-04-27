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
            _currentState = Device.DepthBank.GetPreset(preset);
        }

        /// <summary>Applies the current state to the device. Called internally.</summary>
        internal void Refresh()
        {
            // Ensure the default preset is used if a null state was requested.
            _currentState = _currentState ?? Device.DepthBank.GetPreset(DepthStencilPreset.Default);
            bool stateChanged = _slotState.Bind(Pipe, _currentState);

            if (stateChanged || _stencilRef != _currentState.StencilReference)
            {
                _stencilRef = _currentState.StencilReference;
                Pipe.Context.OutputMerger.SetDepthStencilState(_currentState.State, _stencilRef);
            }
        }

        /// <summary>Gets or sets the current graphics depth-stencil state.</summary>
        public GraphicsDepthState Current
        {
            get { return _currentState; }
            set { _currentState = value; }
        }
    }
}

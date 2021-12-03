using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class GraphicsDepthStage : PipelineComponent<DeviceDX11, PipeDX11>
    {
        PipelineBindSlot<GraphicsDepthState, DeviceDX11, PipeDX11> _slotState;
        GraphicsDepthState _currentState = null;
        int _stencilRef = 0;

        internal GraphicsDepthStage(PipeDX11 pipe) : base(pipe)
        {
            _slotState = AddSlot<GraphicsDepthState>(0);
            _slotState.OnObjectForcedUnbind += _slotState_OnBoundObjectDisposed;
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, PipelineDisposableObject obj)
        {
            Pipe.Context.OutputMerger.DepthStencilState = null;
        }

        protected override void OnDispose()
        {
            _currentState = null;
            base.OnDispose();
        }

        /// <summary>Applies the current state to the device. Called internally.</summary>
        internal void Refresh()
        {
            bool stateChanged = _slotState.Bind(Pipe, _currentState, PipelineBindType.Output);
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

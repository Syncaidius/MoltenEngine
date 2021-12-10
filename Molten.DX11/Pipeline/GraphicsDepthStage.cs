using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal unsafe class GraphicsDepthStage : PipelineComponent<DeviceDX11, PipeDX11>
    {
        PipelineBindSlot<GraphicsDepthState, DeviceDX11, PipeDX11> _slotState;
        GraphicsDepthState _currentState = null;

        ID3D11DepthStencilState* _nativeState;
        uint _stencilRef = 0;

        internal GraphicsDepthStage(PipeDX11 pipe) : base(pipe)
        {
            _slotState = AddSlot<GraphicsDepthState>(0);
            _slotState.OnObjectForcedUnbind += _slotState_OnBoundObjectDisposed;
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, PipelineDisposableObject obj)
        {
            GraphicsDepthState state = obj as GraphicsDepthState;
            if (state._native == _nativeState)
            {
                _nativeState = null;
                _currentState = null;
                _stencilRef = 0;
                Pipe.Context->OMSetDepthStencilState(_nativeState, _stencilRef);
            }
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
                _nativeState = _currentState._native;
                Pipe.Context->OMSetDepthStencilState(_nativeState, _stencilRef);
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

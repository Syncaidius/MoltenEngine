using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class GraphicsBlendStage : PipelineComponent<DeviceDX11, PipeDX11>
    {
        PipelineBindSlot<GraphicsBlendState, DeviceDX11, PipeDX11> _slotState;
        GraphicsBlendState _currentState;

        BlendState _state;
        uint _blendSampleMask;
        Color4 _blendFactor;

        internal GraphicsBlendStage(PipeDX11 context) : base(context)
        {
            _slotState = AddSlot<GraphicsBlendState>(0);
            _slotState.OnObjectForcedUnbind += _slotState_OnBoundObjectDisposed;
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, PipelineDisposableObject obj)
        {
            Pipe.Context.OutputMerger.BlendState = null;
        }

        protected override void OnDispose()
        {
            _currentState = null;
            base.OnDispose();
        }

        internal void Refresh()
        {
            bool stateChanged = _slotState.Bind(Pipe, _currentState, PipelineBindType.Output);

            if (_state != _currentState.Native || _blendFactor != _currentState.BlendFactor || _blendSampleMask != _currentState.BlendSampleMask)
            {
                _blendFactor = _currentState.BlendFactor;
                _blendSampleMask = _currentState.BlendSampleMask;
                Pipe.Context.OutputMerger.SetBlendState(_currentState.Native, _blendFactor.ToRawApi(), _blendSampleMask);
            }
        }

        /// <summary>Gets the currently active blend state.</summary>
        public GraphicsBlendState Current
        {
            get { return _currentState; }
            set { _currentState = value; }
        }
    }
}

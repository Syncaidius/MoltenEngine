using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal unsafe class GraphicsBlendStage : PipelineComponent<DeviceDX11, PipeDX11>
    {
        PipelineBindSlot<GraphicsBlendState, DeviceDX11, PipeDX11> _slotState;
        GraphicsBlendState _currentState;

        ID3D11BlendState* _nativeState;
        uint _blendSampleMask;
        float[] _blendFactor;

        internal GraphicsBlendStage(PipeDX11 context) : base(context)
        {
            _blendFactor = new float[4];
            _slotState = AddSlot<GraphicsBlendState>(0);
            _slotState.OnObjectForcedUnbind += _slotState_OnBoundObjectDisposed;
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, PipelineDisposableObject obj)
        {
            GraphicsBlendState state = obj as GraphicsBlendState;
            if (state._native == _nativeState)
            {
                _nativeState = null;
                Pipe.Context->OMSetBlendState(_nativeState, ref _blendFactor[0], _blendSampleMask);
            }
        }

        protected override void OnDispose()
        {
            _currentState = null;
            base.OnDispose();
        }

        internal void Refresh()
        {
            bool stateChanged = _slotState.Bind(Pipe, _currentState, PipelineBindType.Output);

            if (_nativeState != _currentState._native ||  
                _blendSampleMask != _currentState.BlendSampleMask &&
                !_currentState.BlendFactor.Equals(_blendFactor))
            {
                _nativeState = _currentState._native;
                _currentState.BlendFactor.CopyTo(_blendFactor, 0);
                _blendSampleMask = _currentState.BlendSampleMask; 
                Pipe.Context->OMSetBlendState((ID3D11BlendState*)_nativeState, ref _blendFactor[0], _blendSampleMask);
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

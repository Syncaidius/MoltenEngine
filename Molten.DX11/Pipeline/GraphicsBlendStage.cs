using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class GraphicsBlendStage : PipelineComponent
    {
        PipelineBindSlot<GraphicsBlendState> _slotState;
        GraphicsBlendState _currentState;

        BlendState _state;
        uint _blendSampleMask;
        Color4 _blendFactor;

        internal GraphicsBlendStage(GraphicsPipe context) : base(context)
        {
            _slotState = AddSlot<GraphicsBlendState>(PipelineSlotType.Output, 0);
            _slotState.OnBoundObjectDisposed += _slotState_OnBoundObjectDisposed;
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            Pipe.Context.OutputMerger.BlendState = null;
        }

        protected override void OnDispose()
        {
            _currentState = null;
            base.OnDispose();
        }

        /// <summary>Sets the blending state of the device via a preset.</summary>
        /// <param name="preset"></param>
        public void SetPreset(BlendPreset preset)
        {
            _currentState = Device.GetPreset(preset);
        }

        internal void Refresh()
        {
            _currentState = _currentState ?? Device.GetPreset(BlendPreset.Default);
            bool stateChanged = _slotState.Bind(Pipe, _currentState);

            if (_state != _currentState.State || _blendFactor != _currentState.BlendFactor || _blendSampleMask != _currentState.BlendSampleMask)
            {
                _blendFactor = _currentState.BlendFactor;
                _blendSampleMask = _currentState.BlendSampleMask;
                Pipe.Context.OutputMerger.SetBlendState(_currentState.State, _blendFactor.ToRawApi(), _blendSampleMask);
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

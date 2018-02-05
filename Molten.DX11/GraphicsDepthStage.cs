using SharpDX.Direct3D11;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class GraphicsDepthStage : PipelineComponent
    {
        GraphicsDepthState[] _presets;

        PipelineBindSlot<GraphicsDepthState> _slotState;
        GraphicsDepthState _currentState = null;
        int _stencilRef = 0;
        bool _stencilRefChanged = false;

        internal GraphicsDepthStage(GraphicsPipe pipe) : base(pipe)
        {
            _slotState = AddSlot<GraphicsDepthState>(PipelineSlotType.Output, 0);
            _slotState.OnBoundObjectDisposed += _slotState_OnBoundObjectDisposed;

            //init preset array
            DepthStencilPreset last = EnumHelper.GetLastValue<DepthStencilPreset>();
            int presetArraySize = (int)last + 1;
            _presets = new GraphicsDepthState[presetArraySize];

            //default stencil-enabled preset
            _presets[(int)DepthStencilPreset.Default] = new GraphicsDepthState()
            {
                IsStencilEnabled = true,
            };

            //default preset
            _presets[(int)DepthStencilPreset.DefaultNoStencil] = new GraphicsDepthState();

            //Z-disabled preset
            _presets[(int)DepthStencilPreset.ZDisabled] = new GraphicsDepthState()
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
            };

            _presets[(int)DepthStencilPreset.Sprite2D] = new GraphicsDepthState()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = true,
                DepthComparison = Comparison.LessEqual,
            };
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            Pipe.Context.OutputMerger.DepthStencilState = null;
        }

        public GraphicsDepthState GetPresetState(DepthStencilPreset preset)
        {
            return _presets[(int)preset];
        }

        protected override void OnDispose()
        {
            for (int i = 0; i < _presets.Length; i++)
                _presets[i].Dispose();

            _currentState = null;

            base.OnDispose();
        }

        public void SetPreset(DepthStencilPreset preset)
        {
            _currentState = _presets[(int)preset];
        }

        /// <summary>Applies the current state to the device. Called internally.</summary>
        internal override void Refresh()
        {
            int defaultID = (int)BlendingPreset.Default;

            //ensure the default preset is used if a null state was requested.
            if (_currentState == null)
                _currentState = _presets[defaultID];

            bool changed = _slotState.Bind(Pipe, _currentState);

            if (changed)
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

    public enum DepthStencilPreset
    {
        /// <summary>Default depth stencil state with stencil testing enabled.</summary>
        Default = 0,

        /// <summary>The default depth stencil state, but with stencil testing disabled.</summary>
        DefaultNoStencil = 1,

        /// <summary>The same as default, but with the z-buffer disabled.</summary>
        ZDisabled = 2,

        /// <summary>A state used for drawing 2D sprites. Stenicl testing is enabled.</summary>
        Sprite2D = 3,
    }
}

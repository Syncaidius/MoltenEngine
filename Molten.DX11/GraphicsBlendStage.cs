using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class GraphicsBlendStage : PipelineComponent
    {
        GraphicsBlendState[] _presets;

        PipelineBindSlot<GraphicsBlendState> _slotState;
        GraphicsBlendState _currentState;

        bool _blendFactorDirty;
        int _blendSampleMask;
        Color4 _blendFactor;

        internal GraphicsBlendStage(GraphicsPipe context) : base(context)
        {
            _slotState = AddSlot<GraphicsBlendState>(PipelineSlotType.Output, 0);
            _slotState.OnBoundObjectDisposed += _slotState_OnBoundObjectDisposed;

            //init preset array
            BlendingPreset last = EnumHelper.GetLastValue<BlendingPreset>();
            int presetArraySize = (int)last + 1;
            _presets = new GraphicsBlendState[presetArraySize];

            //default preset
            _presets[(int)BlendingPreset.Default] = new GraphicsBlendState();

            //additive blending preset.
            _presets[(int)BlendingPreset.Additive] = new GraphicsBlendState()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
            };

            //pre-multiplied alpha
            _presets[(int)BlendingPreset.PreMultipliedAlpha] = new GraphicsBlendState()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,

                SourceAlphaBlend = BlendOption.InverseDestinationAlpha,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,

                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                IsBlendEnabled = true,
            };
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            Pipe.Context.OutputMerger.BlendState = null;
        }

        public GraphicsBlendState GetPresetState(BlendingPreset preset)
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

        /// <summary>Sets the blending state of the device via a preset.</summary>
        /// <param name="preset"></param>
        public void SetPreset(BlendingPreset preset)
        {
            _currentState = _presets[(int)preset];
        }

        internal override void Refresh()
        {
            int defaultID = (int)BlendingPreset.Default;

            //ensure the default preset is used if a null state was requested.
            if (_currentState == null)
                _currentState = _presets[defaultID];

            bool changed = _slotState.Bind(Pipe, _currentState);
            if (changed)
                Pipe.Context.OutputMerger.BlendState = _slotState.BoundObject.State;

            if (_blendFactorDirty)
            {
                Pipe.Context.OutputMerger.BlendFactor = _blendFactor.ToRawApi();
                Pipe.Context.OutputMerger.BlendSampleMask = _blendSampleMask;
            }
        }

        /// <summary>
        /// Gets or sets the blend sample mask.
        /// </summary>
        public int BlendSampleMask
        {
            get => _blendSampleMask;
            set
            {
                _blendSampleMask = value;
                _blendFactorDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the blend factor.
        /// </summary>
        public Color4 BlendFactor
        {
            get => _blendFactor;
            set
            {
                _blendFactor = value;
                _blendFactorDirty = true;
            }
        }

        /// <summary>Gets the currently active blend state.</summary>
        public GraphicsBlendState Current
        {
            get { return _currentState; }
            set { _currentState = value; }
        }
    }

    public enum BlendingPreset
    {
        /// <summary>The default blend mode.</summary>
        Default = 0,

        /// <summary>Additive blending mode.</summary>
        Additive = 1,

        /// <summary>Pre-multiplied alpha blending mode.</summary>
        PreMultipliedAlpha = 2,
    }
}

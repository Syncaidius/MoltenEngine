using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Stores a blend state for use with a <see cref="GraphicsPipe"/>.</summary>
    internal class GraphicsBlendState : PipelineObject, IEquatable<GraphicsBlendState>
    {
        internal BlendState State;
        BlendStateDescription _desc;

        bool _dirty;
        Color4 _blendFactor;
        uint _blendSampleMask;

        internal GraphicsBlendState(GraphicsBlendState source)
        {
            _desc = source._desc.Clone();
            _blendFactor = source._blendFactor;
            _blendSampleMask = source._blendSampleMask;
        }

        internal GraphicsBlendState()
        {
            _desc = BlendStateDescription.Default();
            _blendFactor = new Color4(1, 1, 1, 1);
            _blendSampleMask = 0xffffffff;
        }

        internal GraphicsBlendState(RenderTargetBlendDescription rtDesc)
        {
            _desc = BlendStateDescription.Default();
            _desc.RenderTarget[0] = rtDesc;
            _blendFactor = new Color4(1, 1, 1, 1);
            _blendSampleMask = 0xffffffff;
        }

        internal RenderTargetBlendDescription GetSurfaceBlendState(int index)
        {
            return _desc.RenderTarget[index];
        }

        public override bool Equals(object obj)
        {
            if (obj is GraphicsBlendState other)
                return Equals(other);
            else
                return false;
        }

        public bool Equals(GraphicsBlendState other)
        {
            if (_desc.IndependentBlendEnable != other.IndependentBlendEnable)
                return false;

            if (_desc.AlphaToCoverageEnable != other.AlphaToCoverageEnable)
                return false;

            // Equality check against all RT blend states
            for(int i = 0; i < _desc.RenderTarget.Length; i++)
            {
                RenderTargetBlendDescription rt = _desc.RenderTarget[i];
                RenderTargetBlendDescription otherRt = other._desc.RenderTarget[i];

                if (rt.AlphaBlendOperation != otherRt.AlphaBlendOperation ||
                    rt.BlendOperation != otherRt.BlendOperation ||
                    rt.DestinationAlphaBlend != otherRt.DestinationAlphaBlend ||
                    rt.DestinationBlend != otherRt.DestinationBlend ||
                    rt.IsBlendEnabled != otherRt.IsBlendEnabled ||
                    rt.RenderTargetWriteMask != otherRt.RenderTargetWriteMask ||
                    rt.SourceAlphaBlend != otherRt.SourceAlphaBlend ||
                    rt.SourceBlend != otherRt.SourceBlend)
                {
                    return false;
                }
            }
            return true;
        }

        internal override void Refresh(GraphicsPipe context, PipelineBindSlot slot)
        {
            if (State == null || _dirty)
            {
                _dirty = false;

                // Dispose of previous state object
                if (State != null)
                    State.Dispose();

                // Create new state
                State = new BlendState(context.Device.D3d, _desc);
            }
        }

        protected override void OnDispose()
        {
            DisposeObject(ref State);

            base.OnDispose();
        }

        public bool AlphaToCoverageEnable
        {
            get => _desc.AlphaToCoverageEnable;
            set
            {
                _desc.AlphaToCoverageEnable = value;
                _dirty = true;
            }
        }

        public bool IndependentBlendEnable
        {
            get => _desc.IndependentBlendEnable;
            set
            {
                _desc.IndependentBlendEnable = value;
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the blend sample mask.
        /// </summary>
        public uint BlendSampleMask
        {
            get => _blendSampleMask;
            set => _blendSampleMask = value;
        }

        /// <summary>
        /// Gets or sets the blend factor.
        /// </summary>
        public Color4 BlendFactor
        {
            get => _blendFactor;
            set => _blendFactor = value;
        }

        /// <summary>
        /// Gets or sets a render target blend description at the specified index.
        /// </summary>
        /// <param name="rtIndex">The render target/surface blend index.</param>
        /// <returns></returns>
        internal RenderTargetBlendDescription this[int rtIndex]
        {
            get => _desc.RenderTarget[rtIndex];
            set
            {
                _desc.RenderTarget[rtIndex] = value;
                _dirty = true;
            }
        }
    }
}

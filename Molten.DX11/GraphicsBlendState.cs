using SharpDX.Direct3D11;
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
        bool _dirty;
        BlendStateDescription _desc;

        internal GraphicsBlendState()
        {
            _desc = BlendStateDescription.Default();
            _dirty = true;
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

        /// <summary>Sets the blend state for a particular render target slot.</summary>
        /// <param name="id">The slot ID.</param>
        /// <param name="desc">The render target blend description.</param>
        public void SetRenderTargetBlend(int id, RenderTargetBlendDescription desc)
        {
            _desc.RenderTarget[id] = desc;
            _dirty = true;
        }

        protected override void OnDispose()
        {
            DisposeObject(ref State);

            base.OnDispose();
        }

        public bool GetIsBlendEnabled(int rtIndex)
        {
            return _desc.RenderTarget[rtIndex].IsBlendEnabled;
        }

        public void SetIsBlendEnabled(int rtIndex, bool value)
        {
            _desc.RenderTarget[rtIndex].IsBlendEnabled = value;
            _dirty = true;
        }

        public BlendOption GetSourceBlend(int rtIndex)
        {
            return _desc.RenderTarget[rtIndex].SourceBlend;
        }

        public void SetSourceBlend(int rtIndex, BlendOption value)
        {
            _desc.RenderTarget[rtIndex].SourceBlend = value;
            _dirty = true;
        }

        public BlendOption GetDestinationBlend(int rtIndex)
        {
            return _desc.RenderTarget[rtIndex].DestinationBlend;
        }

        public void SetDestinationBlend(int rtIndex, BlendOption value)
        {
            _desc.RenderTarget[rtIndex].DestinationBlend = value;
            _dirty = true;
        }

        public BlendOperation GetBlendOperation(int rtIndex)
        {
            return _desc.RenderTarget[rtIndex].BlendOperation;
        }

        public void SetBlendOperation(int rtIndex, BlendOperation value)
        {
            _desc.RenderTarget[rtIndex].BlendOperation = value;
            _dirty = true;
        }

        public BlendOption GetSourceAlphaBlend(int rtIndex)
        {
            return _desc.RenderTarget[rtIndex].SourceAlphaBlend;
        }

        public void SetSourceAlphaBlend(int rtIndex, BlendOption value)
        {
            _desc.RenderTarget[rtIndex].SourceAlphaBlend = value;
            _dirty = true;
        }

        public BlendOption GetDestinationAlphaBlend(int rtIndex)
        {
            return _desc.RenderTarget[rtIndex].DestinationAlphaBlend;
        }

        public void SetDestinationAlphaBlend(int rtIndex, BlendOption value)
        {
            _desc.RenderTarget[rtIndex].DestinationAlphaBlend = value;
            _dirty = true;
        }

        public BlendOperation GetAlphaBlendOperation(int rtIndex)
        {
            return _desc.RenderTarget[rtIndex].AlphaBlendOperation;
        }

        public void SetAlphaBlendOperation(int rtIndex, BlendOperation value)
        {
            _desc.RenderTarget[rtIndex].AlphaBlendOperation = value;
            _dirty = true;
        }

        public ColorWriteMaskFlags GetRenderTargetWriteMask(int rtIndex)
        {
            return _desc.RenderTarget[rtIndex].RenderTargetWriteMask;
        }

        public void SetRenderTargetWriteMask(int rtIndex, ColorWriteMaskFlags value)
        {
            _desc.RenderTarget[rtIndex].RenderTargetWriteMask = value;
            _dirty = true;
        }

        public void ApplyRenderTargetZeroToAll()
        {
            for (int i = 0; i < _desc.RenderTarget.Length; i++)
                _desc.RenderTarget[i] = _desc.RenderTarget[0];
        }

        public bool IsBlendEnabled
        {
            get => GetIsBlendEnabled(0);
            set => SetIsBlendEnabled(0, value);
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

        public BlendOption SourceBlend
        {
            get => GetSourceBlend(0);
            set => SetSourceBlend(0, value);
        }

        public BlendOption DestinationBlend
        {
            get => GetDestinationBlend(0);
            set => SetDestinationBlend(0, value);
        }

        public BlendOperation BlendOperation
        {
            get => GetBlendOperation(0);
            set => SetBlendOperation(0, value);
        }

        public BlendOption SourceAlphaBlend
        {
            get => GetSourceAlphaBlend(0);
            set => SetSourceAlphaBlend(0, value);
        }

        public BlendOption DestinationAlphaBlend
        {
            get => GetDestinationAlphaBlend(0);
            set => SetDestinationAlphaBlend(0, value);
        }

        public BlendOperation AlphaBlendOperation
        {
            get => GetAlphaBlendOperation(0);
            set => SetAlphaBlendOperation(0, value);
        }

        public ColorWriteMaskFlags RenderTargetWriteMask
        {
            get => GetRenderTargetWriteMask(0);
            set => SetRenderTargetWriteMask(0, value);
        }

        public RenderTargetBlendDescription this[int rtIndex]
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

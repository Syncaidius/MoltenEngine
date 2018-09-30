using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class PipelineOutput : PipelineComponent
    {
        enum SlotType
        {
            RenderSurface = 0,
            DepthBuffer = 1,
        }

        GraphicsPipe _pipe;
        PipelineBindSlot<RenderSurfaceBase>[] _slotSurfaces;
        PipelineBindSlot<DepthSurface> _slotDepth;

        RenderSurfaceBase[] _surfaces;
        DepthSurface _depthSurface = null;
        RenderTargetView[] _rtViews;
        DepthStencilView _depthView = null;

        GraphicsDepthMode _boundMode = GraphicsDepthMode.Enabled;
        GraphicsDepthMode _depthMode = GraphicsDepthMode.Enabled;

        public PipelineOutput(GraphicsPipe pipe) : base(pipe.Device)
        {
            _pipe = pipe;

            int maxRTs = Device.Features.SimultaneousRenderSurfaces;
            _slotSurfaces = new PipelineBindSlot<RenderSurfaceBase>[maxRTs];
            _surfaces = new RenderSurfaceBase[maxRTs];
            _rtViews = new RenderTargetView[maxRTs];

            for (int i = 0; i < maxRTs; i++)
            {
                _slotSurfaces[i] = AddSlot<RenderSurfaceBase>(i);
                _slotSurfaces[i].OnObjectForcedUnbind += SurfaceSlot_OnBoundObjectDisposed;
            }

            _slotDepth = AddSlot<DepthSurface>(0);
            _slotDepth.OnObjectForcedUnbind += _slotDepth_OnBoundObjectDisposed;
        }

        private void SurfaceSlot_OnBoundObjectDisposed(PipelineBindSlotBase slot, PipelineObjectBase obj)
        {
            _rtViews[slot.SlotID] = null;
            _pipe.Context.OutputMerger.SetTargets(_depthView, _rtViews);
            Pipe.Profiler.Current.RTSwaps++;
        }

        private void _slotDepth_OnBoundObjectDisposed(PipelineBindSlotBase slot, PipelineObjectBase obj)
        {
            _depthView = null;
            _pipe.Context.OutputMerger.SetTargets(_depthView, _rtViews);
            Pipe.Profiler.Current.RTSwaps++;
        }

        internal void Refresh()
        {
            bool rtChangeDetected = false;

            // Check depth surface for changes
            bool depthChanged = _slotDepth.Bind(_pipe, _depthSurface, _depthMode == GraphicsDepthMode.ReadOnly ? PipelineBindType.OutputReadOnly : PipelineBindType.Output);
            if (_slotDepth.BoundObject == null)
            {
                _depthView = null;
            }
            else
            {
                DepthStencilView oldDepthView = _depthView;
                _boundMode = _depthMode;

                switch (_depthMode)
                {
                    case GraphicsDepthMode.Disabled:
                        _depthView = null;
                        break;

                    case GraphicsDepthMode.Enabled:
                        _depthView = _slotDepth.BoundObject.DepthView;
                        break;

                    case GraphicsDepthMode.ReadOnly:
                        _depthView = _slotDepth.BoundObject.ReadOnlyView;
                        break;
                }

                depthChanged = depthChanged || _depthView != oldDepthView;
            }

            // Check for render surface changes
            RenderTargetView rtv = null;
            for (int i = 0; i < _surfaces.Length; i++)
            {
                bool rtChanged = _slotSurfaces[i].Bind(_pipe, _surfaces[i], PipelineBindType.Output);
                rtv = _slotSurfaces[i].BoundObject != null ? _slotSurfaces[i].BoundObject.RTV : null;

                if (rtChanged || rtv != _rtViews[i])
                {
                    rtChangeDetected = true;

                    if (_slotSurfaces[i].BoundObject == null)
                        _rtViews[i] = null;
                    else
                        _rtViews[i] = _slotSurfaces[i].BoundObject.RTV;
                }
            }

            // Check if changes need to be forwarded to the GPU.
            if (rtChangeDetected || depthChanged)
            {
                _pipe.Context.OutputMerger.SetTargets(_depthView, _rtViews);
                Pipe.Profiler.Current.RTSwaps++;
            }
        }

        public void SetDepthSurface(DepthSurface surface, GraphicsDepthMode depthMode)
        {
            _depthSurface = surface;
            _depthMode = depthMode;
        }

        public DepthSurface GetDepthSurface()
        {
            return _depthSurface;
        }

        public void SetDepthMode(GraphicsDepthMode value)
        {
            _depthMode = value;
        }

        public GraphicsDepthMode GetDepthMode()
        {
            return _depthMode;
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">The surfaces.</param>
        /// <param name="count">The number of surfaces to set.</param>
        public void SetRenderSurfaces(RenderSurfaceBase[] surfaces, int count)
        {
            if (surfaces != null)
            {
                for (int i = 0; i < count; i++)
                    _surfaces[i] = surfaces[i];
            }
            else
            {
                count = 0;
            }

            // Set the remaining surfaces to null.
            for (int i = count; i < _surfaces.Length; i++)
                _surfaces[i] = null;
        }

        /// <summary>
        /// Sets the render surface.
        /// </summary>
        /// <param name="surface">The surface.</param>
        /// <param name="slot">The slot.</param>
        public void SetRenderSurface(RenderSurfaceBase surface, int slot)
        {
            _surfaces[slot] = surface;
        }

        /// <summary>
        /// Fills the provided array with a list of applied render surfaces.
        /// </summary>
        /// <param name="destinationArray">The array to fill with applied render surfaces.</param>
        public void GetRenderSurfaces(RenderSurfaceBase[] destinationArray)
        {
            for (int i = 0; i < _surfaces.Length; i++)
                destinationArray[i] = _surfaces[i];
        }

        /// <summary>Gets the render surface located in the specified output slot.</summary>
        /// <param name="slot">The ID of the slot to retrieve from.</param>
        /// <returns></returns>
        public RenderSurfaceBase GetRenderSurface(int slot)
        {
            return _surfaces[slot];
        }

        /// <summary>Resets the render surface contained in an output slot.</summary>
        /// <param name="resetMode"></param>
        /// <param name="slot"></param>
        public void ResetRenderSurface(int slot)
        {
            _surfaces[slot] = null;
        }

        /// <summary>
        /// Resets the render surfaces.
        /// </summary>
        /// <param name="resetMode">The reset mode.</param>
        /// <param name="outputOnFirst">If true and the reset mode is OutputSurface, it will only be applied to the first slot (0)..</param>
        public void ResetRenderSurfaces()
        {
            for (int i = 0; i < _surfaces.Length; i++)
                _surfaces[i] = null;
        }

        /// <summary>Clears a render target that is set on the device.</summary>
        /// <param name="color"></param>
        /// <param name="slot"></param>
        public void Clear(Color color, int slot)
        {
            if (_surfaces[slot] != null)
                _surfaces[slot].Clear(_pipe, color);
        }

        internal GraphicsValidationResult Validate()
        {
            // TODO if render target 0 is set, ensure a pixel shader is bound, otherwise flag as missing pixel shader.

            return GraphicsValidationResult.Successful;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }

        internal override bool IsValid { get { return true; } }

        /// <summary>
        /// Gets whether or not a render target has been applied at slot 0.
        /// </summary>
        internal bool TargetZeroSet { get { return _rtViews[0] != null; } }

        /// <summary>
        /// Gets the <see cref="RenderSurfaceBase"/> at the specified slot.
        /// </summary>
        public RenderSurfaceBase this[int slotIndex]
        {
            get { return _surfaces[slotIndex]; }
        }
    }
}

using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class GraphicsRasterizerStage : PipelineComponent
    {
        PipelineBindSlot<GraphicsRasterizerState> _slotState;
        GraphicsRasterizerState _currentState = null;

        SharpDX.Rectangle[] _apiScissorRects;
        Rectangle[] _scissorRects;
        bool _scissorRectsDirty;

        SharpDX.Mathematics.Interop.RawViewportF[] _apiViewports;
        ViewportF[] _viewports;
        bool _viewportsDirty;

        ViewportF[] _nullViewport;

        internal GraphicsRasterizerStage(GraphicsPipe pipe) : base(pipe)
        {
            _nullViewport = new ViewportF[1];

            int maxRTs = pipe.Device.Features.SimultaneousRenderSurfaces;
            _scissorRects = new Rectangle[maxRTs];
            _viewports = new ViewportF[maxRTs];
            _apiScissorRects = new SharpDX.Rectangle[maxRTs];
            _apiViewports = new SharpDX.Mathematics.Interop.RawViewportF[maxRTs];

            _slotState = AddSlot<GraphicsRasterizerState>(PipelineSlotType.Output, 0);
            _slotState.OnBoundObjectDisposed += _slotState_OnBoundObjectDisposed;
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            Pipe.Context.Rasterizer.State = null;
        }

        protected override void OnDispose()
        {
            _currentState = null;

            base.OnDispose();
        }

        /// <summary>Sets the blending state of the device via a preset.</summary>
        /// <param name="preset"></param>
        public void SetPreset(RasterizerPreset preset)
        {
            _currentState = Device.GetPreset(preset);
        }

        public void SetScissorRectangle(Rectangle rect, int slot = 0)
        {
            _scissorRects[slot] = rect;
            _scissorRectsDirty = true;
        }

        public void SetScissorRectangles(Rectangle[] rects)
        {
            for (int i = 0; i < rects.Length; i++)
                _scissorRects[i] = rects[i];

            // Reset any remaining scissor rectangles to whatever the first is.
            for (int i = rects.Length; i < _scissorRects.Length; i++)
                _scissorRects[i] = _scissorRects[0];

            _scissorRectsDirty = true;
        }

        /// <summary>
        /// Applies the provided viewport value to the specified viewport slot.
        /// </summary>
        /// <param name="vp">The viewport value.</param>
        public void SetViewport(ViewportF vp, int slot)
        {
                _viewports[slot] = vp;
        }

        /// <summary>
        /// Applies the specified viewport to all viewport slots.
        /// </summary>
        /// <param name="vp">The viewport value.</param>
        public void SetViewports(ViewportF vp)
        {
            for (int i = 0; i < _viewports.Length; i++)
                _viewports[i] = vp;

            _viewportsDirty = true;
        }

        /// <summary>
        /// Sets the provided viewports on to their respective viewport slots. <para/>
        /// If less than the total number of viewport slots was provided, the remaining ones will be set to whatever the same value as the first viewport slot.
        /// </summary>
        /// <param name="viewports"></param>
        public void SetViewports(ViewportF[] viewports)
        {
            if (viewports == null)
            {
                RenderSurfaceBase surface = null;
                RenderSurfaceBase surfaceZero = Pipe.Output.GetRenderSurface(0);

                for (int i = 0; i < _viewports.Length; i++)
                {
                    surface = Pipe.Output.GetRenderSurface(i);
                    _viewports[i] = surface != null ? surface.Viewport : surfaceZero.Viewport;
                }
            }
            else
            {
                for (int i = 0; i < viewports.Length; i++)
                    _viewports[i] = viewports[i];

                // Set remaining unset ones to whatever the first is.
                for (int i = viewports.Length; i < _viewports.Length; i++)
                    _viewports[i] = _viewports[0];
            }

            _viewportsDirty = true;
        }

        public void GetViewports(ViewportF[] outArray)
        {
            Array.Copy(_viewports, outArray, _viewports.Length);
        }

        public ViewportF GetViewport(int index)
        {
            return _viewports[index];
        }

        /// <summary>Applies the current state to the device. Called internally.</summary>
        internal override void Refresh()
        {
            // Ensure the default preset is used if a null state was requested.
            _currentState = _currentState ?? Device.GetPreset(RasterizerPreset.Default);
            bool stateChanged = _slotState.Bind(Pipe, _currentState);

            if (stateChanged)   // Update rasterizer state.
                Pipe.Context.Rasterizer.State = _slotState.BoundObject.State;

            // Check if scissor rects need updating
            if (_scissorRectsDirty)
            {
                for (int i = 0; i < _scissorRects.Length; i++)
                    _apiScissorRects[i] = _scissorRects[i].ToApi();

                Pipe.Context.Rasterizer.SetScissorRectangles(_apiScissorRects);
                _scissorRectsDirty = false;
            }

            // Check if viewports need updating.
            if (_viewportsDirty)
            {
                for (int i = 0; i < _viewports.Length; i++)
                    _apiViewports[i] = _viewports[i].ToRawApi();

                Pipe.Context.Rasterizer.SetViewports(_apiViewports, _viewports.Length);
                _viewportsDirty = false;
            }
        }

        /// <summary>Gets the currently active blend state.</summary>
        public GraphicsRasterizerState Current
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        /// <summary>Gets the number of applied viewports.</summary>
        public int ViewportCount
        {
            get { return _viewports.Length; }
        }
    }
}

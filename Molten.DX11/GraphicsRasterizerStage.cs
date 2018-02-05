using SharpDX;
using SharpDX.Direct3D11;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class GraphicsRasterizerStage : PipelineComponent
    {
        GraphicsRasterizerState[] _presets;

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

            //init preset array
            RasterizerPreset last = EnumHelper.GetLastValue<RasterizerPreset>();
            int presetArraySize = (int)last + 1;
            _presets = new GraphicsRasterizerState[presetArraySize];

            //default preset
            _presets[(int)RasterizerPreset.Default] = new GraphicsRasterizerState();

            //wireframe preset.
            _presets[(int)RasterizerPreset.Wireframe] = new GraphicsRasterizerState()
            {
                FillMode = FillMode.Wireframe,
            };

            //scissor test preset
            _presets[(int)RasterizerPreset.ScissorTest] = new GraphicsRasterizerState()
            {
                IsScissorEnabled = true,
            };

            //no culling preset.
            _presets[(int)RasterizerPreset.NoCulling] = new GraphicsRasterizerState()
            {
                CullMode = CullMode.None,
            };
        }

        private void _slotState_OnBoundObjectDisposed(PipelineBindSlot slot, PipelineObject obj)
        {
            Pipe.Context.Rasterizer.State = null;
        }

        public GraphicsRasterizerState GetPresetState(RasterizerPreset preset)
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
        public void SetPreset(RasterizerPreset preset)
        {
            _currentState = _presets[(int)preset];
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

        public void SetViewports(params ViewportF[] viewports)
        {
            if (viewports == null)
            {
                RenderSurfaceBase surface = null;
                RenderSurfaceBase surfaceZero = Pipe.Output.GetRenderSurface(0);

                for (int i = 0; i < _viewports.Length; i++)
                {
                    surface = Pipe.Output.GetRenderSurface(i);
                    _viewports[i] = surface != null ? surface.Viewport : surfaceZero.Viewport; // TODO set this to whatever the viewport of each render target is (iterate over them).
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
            int defaultID = (int)BlendingPreset.Default;

            //ensure the default preset is used if a null state was requested.
            if (_currentState == null)
                _currentState = _presets[defaultID];

            // Update rasterizer state.
            bool stateChanged = _slotState.Bind(Pipe, _currentState);
            if (stateChanged)
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

    /// <summary>Represents several rasterizer state presets.</summary>
    public enum RasterizerPreset
    {
        /// <summary>The default rasterizer state.</summary>
        Default = 0,

        /// <summary>The same as the default rasterizer state, but with wireframe enabled.</summary>
        Wireframe = 1,

        /// <summary>The same as the default rasterizer state, but with scissor testing enabled.</summary>
        ScissorTest = 2,

        /// <summary>Culling is disabled. Back and front faces will be drawn.</summary>
        NoCulling = 3,
    }
}

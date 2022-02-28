using Silk.NET.Direct3D11;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal unsafe class GraphicsRasterizerStage : PipeStateStage<GraphicsRasterizerState, ID3D11RasterizerState>
    {
        Silk.NET.Maths.Rectangle<int>[] _apiScissorRects;
        Rectangle[] _scissorRects;
        bool _scissorRectsDirty;

        Silk.NET.Direct3D11.Viewport[] _apiViewports;
        ViewportF[] _viewports;
        bool _viewportsDirty;
        ViewportF[] _nullViewport;

        internal GraphicsRasterizerStage(DeviceContext pipe) : base(pipe)
        {
            _nullViewport = new ViewportF[1];

            uint maxRTs = pipe.Device.Features.SimultaneousRenderSurfaces;
            _scissorRects = new Rectangle[maxRTs];
            _viewports = new ViewportF[maxRTs];
            _apiScissorRects = new Silk.NET.Maths.Rectangle<int>[maxRTs];
            _apiViewports = new Silk.NET.Direct3D11.Viewport[maxRTs];
        }

        protected override void UnbindState(PipeSlot<GraphicsRasterizerState> slot)
        {
            Pipe.Native->RSSetState(null);
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
                RenderSurface surface = null;
                RenderSurface surfaceZero = Pipe.Output.GetRenderSurface(0);

                for (uint i = 0; i < _viewports.Length; i++)
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

        protected override void BindState(GraphicsRasterizerState state)
        {
            state = state ?? Device.RasterizerBank.GetPreset(RasterizerPreset.Default);
            Pipe.Native->RSSetState(state);
        }

        public new void Bind()
        {
            base.Bind();

            // Check if scissor rects need updating
            if (_scissorRectsDirty)
            {
                for (int i = 0; i < _scissorRects.Length; i++)
                    _apiScissorRects[i] = _scissorRects[i].ToApi();

                fixed (Rectangle<int>* ptrRect = _apiScissorRects)
                    Pipe.Native->RSSetScissorRects((uint)_apiScissorRects.Length, ptrRect);

                _scissorRectsDirty = false;
            }

            // Check if viewports need updating.
            if (_viewportsDirty)
            {
                for (int i = 0; i < _viewports.Length; i++)
                    _apiViewports[i] = _viewports[i].ToApi();

                Pipe.Native->RSSetViewports((uint)_viewports.Length, ref _apiViewports[0]);
                _viewportsDirty = false;
            }
        }

        /// <summary>Gets the number of applied viewports.</summary>
        public int ViewportCount => _viewports.Length;
    }
}

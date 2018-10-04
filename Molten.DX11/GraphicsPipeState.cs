using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Stores the current state of a <see cref="GraphicsPipe"/>.</summary>
    internal class GraphicsPipeState
    {
        GraphicsPipe _pipe;
        GraphicsBlendState _blendState;
        GraphicsDepthState _depthState;
        GraphicsRasterizerState _rasterState;

        BufferSegment[] _vSegments;
        BufferSegment _iSegment;

        RenderSurfaceBase[] _surfaces;

        DepthSurface _depthSurface;
        GraphicsDepthWritePermission _depthWriteOverride;

        ViewportF[] _viewports;

        public GraphicsPipeState(GraphicsPipe pipe)
        {
            _pipe = pipe;
            int maxSurfaces = _pipe.Device.Features.SimultaneousRenderSurfaces;

            _surfaces = new RenderSurfaceBase[maxSurfaces];
            _viewports = new ViewportF[maxSurfaces];
            _vSegments = new BufferSegment[_pipe.Device.Features.MaxVertexBufferSlots];
        }

        public void Capture()
        {
            _blendState = _pipe.BlendState.Current;
            _depthState = _pipe.DepthStencil.Current;
            _rasterState = _pipe.Rasterizer.Current;

            _pipe.GetVertexSegments(_vSegments);
            _iSegment = _pipe.GetIndexSegment();

            //store viewports
            int vpCount = _pipe.Rasterizer.ViewportCount;
            if (_viewports.Length < vpCount)
                Array.Resize(ref _viewports, vpCount);
            _pipe.Rasterizer.GetViewports(_viewports);

            // Store surfaces
            _pipe.GetRenderSurfaces(_surfaces);

            _depthSurface = _pipe.DepthSurface;
            _depthWriteOverride = _pipe.DepthWriteOverride;
        }

        public void Restore()
        {
            //states
            _pipe.BlendState.Current = _blendState;
            _pipe.DepthStencil.Current = _depthState;
            _pipe.Rasterizer.Current = _rasterState;

            //buffers
            _pipe.SetVertexSegments(_vSegments);
            _pipe.SetIndexSegment(_iSegment);

            //restore viewports
            _pipe.Rasterizer.SetViewports(_viewports);

            // Restore surfaces -- ensure surface 0 is correctly handled when null.
            _pipe.SetRenderSurfaces(_surfaces);
            if (_surfaces[0] == null)
                _pipe.UnsetRenderSurface(0);


            _pipe.DepthSurface = _depthSurface;
            _pipe.DepthWriteOverride = _depthWriteOverride;
        }

        /// <summary>Resets the pipe state, but does not apply it.</summary>
        public void Clear()
        {
            _blendState = null;
            _depthState = null;
            _rasterState = null;

            for (int i = 0; i < _vSegments.Length; i++)
                _vSegments[i] = null;

            _iSegment = null;

            _depthSurface = null;

            for (int i = 0; i < _surfaces.Length; i++)
                _surfaces[i] = null;
        }
    }
}

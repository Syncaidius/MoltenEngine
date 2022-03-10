using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Stores the current state of a <see cref="DeviceContext"/>.</summary>
    internal class GraphicsPipeState
    {
        DeviceContext _pipe;
        GraphicsBlendState _blendState;
        GraphicsDepthState _depthState;
        GraphicsRasterizerState _rasterState;

        BufferSegment[] _vSegments;
        BufferSegment _iSegment;

        RenderSurface[] _surfaces;

        DepthStencilSurface _depthSurface;
        GraphicsDepthWritePermission _depthWriteOverride;

        ViewportF[] _viewports;

        public GraphicsPipeState(DeviceContext pipe)
        {
            _pipe = pipe;
            uint maxSurfaces = _pipe.Device.Features.SimultaneousRenderSurfaces;

            _surfaces = new RenderSurface[maxSurfaces];
            _viewports = new ViewportF[maxSurfaces];
            _vSegments = new BufferSegment[_pipe.Device.Features.MaxVertexBufferSlots];
        }

        public void Capture()
        {
            _blendState = _pipe.State.Blend.Value;
            _depthState = _pipe.State.Depth.Value;
            _rasterState = _pipe.State.Rasterizer.Value;

            _pipe.State.VertexBuffers.Get(_vSegments);
            _iSegment = _pipe.State.IndexBuffer.Value;

            //store viewports
            int vpCount = _pipe.State.ViewportCount;
            if (_viewports.Length < vpCount)
                Array.Resize(ref _viewports, vpCount);
            _pipe.State.GetViewports(_viewports);

            // Store surfaces
            _pipe.State.GetRenderSurfaces(_surfaces);

            _depthSurface = _pipe.State.DepthSurface.Value;
            _depthWriteOverride = _pipe.State.DepthWriteOverride;
        }

        public void Restore()
        {
            //states
            _pipe.State.Blend.Value = _blendState;
            _pipe.State.Depth.Value = _depthState;
            _pipe.State.Rasterizer.Value = _rasterState;

            //buffers
            _pipe.State.VertexBuffers.Set(_vSegments);
            _pipe.State.IndexBuffer.Value = _iSegment;

            //restore viewports
            _pipe.State.SetViewports(_viewports);

            // Restore surfaces -- ensure surface 0 is correctly handled when null.
            _pipe.State.SetRenderSurfaces(_surfaces);
            if (_surfaces[0] == null)
                _pipe.State.SetRenderSurface(null, 0);


            _pipe.State.DepthSurface.Value = _depthSurface;
            _pipe.State.DepthWriteOverride = _depthWriteOverride;
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

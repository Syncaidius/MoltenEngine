namespace Molten.Graphics
{
    /// <summary>Stores the current state of a <see cref="CommandQueueDX11"/>.</summary>
    internal class GraphicsState
    {
        CommandQueueDX11 _cmd;
        GraphicsBlendState _blendState;
        GraphicsDepthState _depthState;
        GraphicsRasterizerState _rasterState;

        BufferSegment[] _vSegments;
        BufferSegment _iSegment;

        RenderSurface2D[] _surfaces;

        DepthStencilSurface _depthSurface;
        GraphicsDepthWritePermission _depthWriteOverride;

        ViewportF[] _viewports;

        public GraphicsState(CommandQueueDX11 cmd)
        {
            _cmd = cmd;
            uint maxSurfaces = _cmd.DXDevice.Adapter.Capabilities.PixelShader.MaxOutResources;

            _surfaces = new RenderSurface2D[maxSurfaces];
            _viewports = new ViewportF[maxSurfaces];
            _vSegments = new BufferSegment[_cmd.DXDevice.Adapter.Capabilities.VertexBuffers.MaxSlots];
        }

        public void Capture()
        {
            _blendState = _cmd.Blend.Value;
            _depthState = _cmd.Depth.Value;
            _rasterState = _cmd.Rasterizer.Value;

            _cmd.VertexBuffers.Get(_vSegments);
            _iSegment = _cmd.IndexBuffer.Value;

            //store viewports
            int vpCount = _cmd.ViewportCount;
            if (_viewports.Length < vpCount)
                Array.Resize(ref _viewports, vpCount);
            _cmd.GetViewports(_viewports);

            // Store surfaces
            _cmd.GetRenderSurfaces(_surfaces);

            _depthSurface = _cmd.DepthSurface.Value;
            _depthWriteOverride = _cmd.DepthWriteOverride;
        }

        public void Restore()
        {
            //states
            _cmd.Blend.Value = _blendState;
            _cmd.Depth.Value = _depthState;
            _cmd.Rasterizer.Value = _rasterState;

            //buffers
            _cmd.VertexBuffers.Set(_vSegments);
            _cmd.IndexBuffer.Value = _iSegment;

            //restore viewports
            _cmd.SetViewports(_viewports);

            // Restore surfaces -- ensure surface 0 is correctly handled when null.
            _cmd.SetRenderSurfaces(_surfaces);
            if (_surfaces[0] == null)
                _cmd.SetRenderSurface(null, 0);


            _cmd.DepthSurface.Value = _depthSurface;
            _cmd.DepthWriteOverride = _depthWriteOverride;
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

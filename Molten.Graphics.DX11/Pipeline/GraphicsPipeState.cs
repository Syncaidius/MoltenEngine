namespace Molten.Graphics
{
    /// <summary>Stores the current state of a <see cref="DeviceContext"/>.</summary>
    internal class GraphicsPipeState
    {
        DeviceContext _context;
        GraphicsBlendState _blendState;
        GraphicsDepthState _depthState;
        GraphicsRasterizerState _rasterState;

        BufferSegment[] _vSegments;
        BufferSegment _iSegment;

        RenderSurface2D[] _surfaces;

        DepthStencilSurface _depthSurface;
        GraphicsDepthWritePermission _depthWriteOverride;

        ViewportF[] _viewports;

        public GraphicsPipeState(DeviceContext context)
        {
            _context = context;
            uint maxSurfaces = _context.Device.Adapter.Capabilities.PixelShader.MaxOutResources;

            _surfaces = new RenderSurface2D[maxSurfaces];
            _viewports = new ViewportF[maxSurfaces];
            _vSegments = new BufferSegment[_context.Device.Adapter.Capabilities.VertexBuffers.MaxSlots];
        }

        public void Capture()
        {
            _blendState = _context.State.Blend.Value;
            _depthState = _context.State.Depth.Value;
            _rasterState = _context.State.Rasterizer.Value;

            _context.State.VertexBuffers.Get(_vSegments);
            _iSegment = _context.State.IndexBuffer.Value;

            //store viewports
            int vpCount = _context.State.ViewportCount;
            if (_viewports.Length < vpCount)
                Array.Resize(ref _viewports, vpCount);
            _context.State.GetViewports(_viewports);

            // Store surfaces
            _context.State.GetRenderSurfaces(_surfaces);

            _depthSurface = _context.State.DepthSurface.Value;
            _depthWriteOverride = _context.State.DepthWriteOverride;
        }

        public void Restore()
        {
            //states
            _context.State.Blend.Value = _blendState;
            _context.State.Depth.Value = _depthState;
            _context.State.Rasterizer.Value = _rasterState;

            //buffers
            _context.State.VertexBuffers.Set(_vSegments);
            _context.State.IndexBuffer.Value = _iSegment;

            //restore viewports
            _context.State.SetViewports(_viewports);

            // Restore surfaces -- ensure surface 0 is correctly handled when null.
            _context.State.SetRenderSurfaces(_surfaces);
            if (_surfaces[0] == null)
                _context.State.SetRenderSurface(null, 0);


            _context.State.DepthSurface.Value = _depthSurface;
            _context.State.DepthWriteOverride = _depthWriteOverride;
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

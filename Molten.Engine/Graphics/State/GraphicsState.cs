
namespace Molten.Graphics
{
    public class GraphicsState
    {
        internal GraphicsState(GraphicsDevice device)
        {
            Device = device;

            uint maxRTs = Device.Capabilities.PixelShader.MaxOutputTargets;
            Viewports = new GraphicsStateArray<ViewportF>(maxRTs);
            ScissorRects = new GraphicsStateArray<Rectangle>(maxRTs); // TODO More than one scissor rect can be used during a single draw call to the same output surface.
            Surfaces = new GraphicsStateValueGroup<IRenderSurface2D>(maxRTs);
            DepthSurface = new GraphicsStateValue<IDepthStencilSurface>();
            Shader = new GraphicsStateValue<HlslShader>();

            /* TODO:
             *  - Rewrite GraphicsStateValueGroup so that it contains separate _values, _boundValues and _boundVersions arrays
             *      - Add Get/set indexer for directly setting _values
             *      - Add getter property named 'BoundValues' for retrieving _boundValues
             *  - Take control of shader stage slots so that we're able to reset and re-bind resources/buffers directly.
             *      - Aim to fix spam of on example menu:
             *          [DX11 DEBUG] [Frame 1300] [MessageSeverityWarning] [MessageCategoryStateSetting] ID3D11DeviceContext::OMSetRenderTargets: Resource being set to OM RenderTarget slot 0 is still bound on input!
                        [DX11 DEBUG] [Frame 1300] [MessageSeverityWarning] [MessageCategoryStateSetting] ID3D11DeviceContext::OMSetRenderTargets[AndUnorderedAccessViews]: Forcing PS shader resource slot 1 to NULL.
             */
        }

        internal GraphicsState Clone()
        {
            GraphicsState clone = new GraphicsState(Device);
            CopyTo(clone);
            return clone;
        }

        internal void CopyTo(GraphicsState target)
        {
            Shader.CopyTo(target.Shader);
            Viewports.CopyTo(target.Viewports);
            ScissorRects.CopyTo(target.ScissorRects);
            Surfaces.CopyTo(target.Surfaces);
            DepthSurface.CopyTo(target.DepthSurface);
        }

        public void Reset()
        {
            Surfaces.Reset();
            Viewports.Reset();
            ScissorRects.Reset();

            DepthSurface.Value = null;
            Shader.Value = null;
        }

        public GraphicsDevice Device { get; }

        public GraphicsStateArray<ViewportF> Viewports { get; }

        public GraphicsStateArray<Rectangle> ScissorRects { get; }

        public GraphicsStateValueGroup<IRenderSurface2D> Surfaces { get; }

        public GraphicsStateValue<IDepthStencilSurface> DepthSurface { get; }

        public GraphicsStateValue<HlslShader> Shader { get; }
    }
}

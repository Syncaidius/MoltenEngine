namespace Molten.Graphics
{
    internal class StartStep : RenderStep
    {
        public override void Dispose() { }

        internal override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            IRenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            IRenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];
            IRenderSurface2D sEmissive = renderer.Surfaces[MainSurfaceType.Emissive];
            IDepthStencilSurface sDepth = renderer.Surfaces.GetDepth();

            GraphicsCommandQueue cmd = renderer.Device.Cmd;

            cmd.SetRenderSurfaces(null);
            sScene.Clear(camera.BackgroundColor, GraphicsPriority.Immediate);
            sNormals.Clear(Color.White * 0.5f, GraphicsPriority.Immediate);
            sEmissive.Clear(Color.Black, GraphicsPriority.Immediate);
            sDepth.Clear(DepthClearFlags.Depth | DepthClearFlags.Stencil, GraphicsPriority.Immediate, 1, 0);

            renderer.SpriteBatch.Reset((Rectangle)camera.Surface.Viewport.Bounds);
        }
    }
}

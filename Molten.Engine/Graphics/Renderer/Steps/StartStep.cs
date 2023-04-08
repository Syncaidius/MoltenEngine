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

            GraphicsQueue cmd = renderer.Device.Queue;

            cmd.SetRenderSurfaces(null);
            sScene.Clear(GraphicsPriority.Immediate, camera.BackgroundColor);
            sNormals.Clear(GraphicsPriority.Immediate, Color.White * 0.5f);
            sEmissive.Clear(GraphicsPriority.Immediate, Color.Black);
            sDepth.Clear(GraphicsPriority.Immediate, DepthClearFlags.Depth | DepthClearFlags.Stencil, 1, 0);

            renderer.SpriteBatch.Reset((Rectangle)camera.Surface.Viewport.Bounds);
        }
    }
}

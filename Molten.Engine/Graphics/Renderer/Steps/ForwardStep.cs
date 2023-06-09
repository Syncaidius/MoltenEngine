namespace Molten.Graphics
{
    /// <summary>
    /// The forward-rendering step.
    /// </summary>
    internal class ForwardStep : RenderStep
    {
        public override void Dispose()
        { }

        internal override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            if (context.Layer.Renderables.Count == 0)
                return;

            GraphicsQueue cmd = renderer.Device.Queue;
            IRenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];

            cmd.State.Surfaces.Reset();
            cmd.State.Surfaces[0] = sScene;
            cmd.State.DepthSurface.Value = renderer.Surfaces.GetDepth();

            cmd.State.Viewports.Reset(camera.Surface.Viewport);
            cmd.State.ScissorRects.Reset((Rectangle)camera.Surface.Viewport.Bounds);

            renderer.RenderSceneLayer(cmd, context.Layer, camera);
        }
    }
}

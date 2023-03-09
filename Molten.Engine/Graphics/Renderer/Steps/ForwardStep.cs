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

            GraphicsCommandQueue cmd = renderer.Device.Cmd;
            IRenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            cmd.SetRenderSurface(sScene, 0);
            cmd.DepthSurface.Value = renderer.Surfaces.GetDepth();
            cmd.SetViewports(camera.Surface.Viewport);
            cmd.SetScissorRectangle((Rectangle)camera.Surface.Viewport.Bounds);

            cmd.BeginDraw();
            renderer.RenderSceneLayer(cmd, context.Layer, camera);
            cmd.EndDraw();
        }
    }
}

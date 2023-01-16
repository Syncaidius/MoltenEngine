namespace Molten.Graphics
{
    /// <summary>
    /// The forward-rendering step.
    /// </summary>
    internal class ForwardStep : RenderStepBase
    {
        public override void Dispose()
        { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            RenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];

            CommandQueueDX11 cmd = renderer.Device.Cmd;
            sScene.Clear(Color.Transparent);

            cmd.State.SetRenderSurface(sScene, 0);
            cmd.State.DepthSurface.Value = renderer.Surfaces.GetDepth();
            cmd.State.SetViewports(camera.Surface.Viewport);
            cmd.State.SetScissorRectangle((Rectangle)camera.Surface.Viewport.Bounds);

            StateConditions conditions = context.BaseStateConditions | StateConditions.ScissorTest;

            cmd.BeginDraw(conditions);
            renderer.RenderSceneLayer(cmd, context.Layer, camera);
            cmd.EndDraw();
        }
    }
}

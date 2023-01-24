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
            IRenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];

            CommandQueueDX11 cmd = renderer.NativeDevice.Cmd;
            sScene.Clear(Color.Transparent, GraphicsPriority.Immediate);

            cmd.SetRenderSurface(sScene, 0);
            cmd.DepthSurface.Value = renderer.Surfaces.GetDepth() as DepthStencilSurface;
            cmd.SetViewports(camera.Surface.Viewport);
            cmd.SetScissorRectangle((Rectangle)camera.Surface.Viewport.Bounds);

            StateConditions conditions = context.BaseStateConditions | StateConditions.ScissorTest;

            cmd.BeginDraw(conditions);
            renderer.RenderSceneLayer(cmd, context.Layer, camera);
            cmd.EndDraw();
        }
    }
}

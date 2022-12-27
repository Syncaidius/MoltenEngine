namespace Molten.Graphics
{
    /// <summary>
    /// The forward-rendering step.
    /// </summary>
    internal class ForwardStep : RenderStepBase
    {
        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext cxt, Timing time)
        {
            RenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            sScene.Clear(Color.Transparent);

            cxt.Context.State.SetRenderSurface(sScene, 0);
            cxt.Context.State.DepthSurface.Value = renderer.Surfaces.GetDepth();
            cxt.Context.State.SetViewports(camera.Surface.Viewport);
            cxt.Context.State.SetScissorRectangle((Rectangle)camera.Surface.Viewport.Bounds);

            StateConditions conditions = cxt.BaseStateConditions | StateConditions.ScissorTest;

            cxt.Context.BeginDraw(conditions);
            renderer.RenderSceneLayer(cxt.Context, cxt.Layer, camera);
            cxt.Context.EndDraw();
        }
    }
}

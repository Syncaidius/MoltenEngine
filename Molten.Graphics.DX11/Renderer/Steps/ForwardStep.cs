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

            DeviceDX11 device = renderer.Device;
            sScene.Clear(Color.Transparent);

            device.State.SetRenderSurface(sScene, 0);
            device.State.DepthSurface.Value = renderer.Surfaces.GetDepth();
            device.State.SetViewports(camera.Surface.Viewport);
            device.State.SetScissorRectangle((Rectangle)camera.Surface.Viewport.Bounds);

            StateConditions conditions = context.BaseStateConditions | StateConditions.ScissorTest;

            device.BeginDraw(conditions);
            renderer.RenderSceneLayer(device, context.Layer, camera);
            device.EndDraw();
        }
    }
}

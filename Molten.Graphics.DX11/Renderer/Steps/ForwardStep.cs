namespace Molten.Graphics
{
    /// <summary>
    /// The forward-rendering step.
    /// </summary>
    internal class ForwardStep : RenderStepBase
    {
        RenderSurface2D _surfaceScene;
        DepthStencilSurface _surfaceDepth;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.Surfaces[MainSurfaceType.Scene];
            _surfaceDepth = renderer.Surfaces.GetDepth();
        }

        public override void Dispose()
        { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            Device device = renderer.Device;
            _surfaceScene.Clear(Color.Transparent);

            device.State.SetRenderSurface(_surfaceScene, 0);
            device.State.DepthSurface.Value = _surfaceDepth;
            device.State.SetViewports(camera.OutputSurface.Viewport);
            device.State.SetScissorRectangle((Rectangle)camera.OutputSurface.Viewport.Bounds);

            StateConditions conditions = StateConditions.ScissorTest; // TODO expand
            conditions |= camera.OutputSurface.MultiSampleLevel >= AntiAliasLevel.X2 ? StateConditions.Multisampling : StateConditions.None;

            device.BeginDraw(conditions);
            renderer.RenderSceneLayer(device, context.Layer, camera);
            device.EndDraw();
        }
    }
}

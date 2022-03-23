namespace Molten.Graphics
{
    internal class FinalizeStep : RenderStepBase
    {
        RenderCamera _orthoCamera;
        ObjectRenderData _dummyData;

        RenderSurface2D _surfaceScene;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.Surfaces[MainSurfaceType.Scene];

            _dummyData = new ObjectRenderData();
            _orthoCamera = new RenderCamera(RenderCameraMode.Orthographic);
        }

        public override void Dispose()
        {

        }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            _orthoCamera.OutputSurface = camera.OutputSurface;

            RectangleUI bounds = new RectangleUI(0, 0, camera.OutputSurface.Width, camera.OutputSurface.Height);
            Device device = renderer.Device;
            RenderSurface2D finalSurface = camera.OutputSurface as RenderSurface2D;
            if (!camera.HasFlags(RenderCameraFlags.DoNotClear))
                renderer.ClearIfFirstUse(device, finalSurface, context.Scene.BackgroundColor);

            device.State.SetRenderSurfaces(null);
            device.State.SetRenderSurface(finalSurface, 0);
            device.State.DepthSurface.Value = null;
            device.State.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            device.State.SetViewports(camera.OutputSurface.Viewport);
            device.State.SetScissorRectangle((Rectangle)camera.OutputSurface.Viewport.Bounds);

            StateConditions conditions = StateConditions.ScissorTest;
            conditions |= camera.OutputSurface.MultiSampleLevel >= AntiAliasLevel.X2 ? StateConditions.Multisampling : StateConditions.None;

            /* TODO Refactor MSAA renderer support:
             *  - AntiAliasLevel should be stored in SceneRenderData
             *  - If SceneRenderData.MultisampleLevel is set to .None, we use GraphicsSettings.MSAA instead.
             *  - Have a cleanup procedure in the renderer to delete surfaces that are not used for 10 frames.
             */

            renderer.Device.BeginDraw(conditions); // TODO correctly use pipe + conditions here.

            ITexture2D sourceSurface = context.HasComposed ? context.PreviousComposition : _surfaceScene;
            renderer.SpriteBatcher.Draw(sourceSurface, bounds, Vector2F.Zero, camera.OutputSurface.Viewport.Bounds.Size, Color.White, 0, Vector2F.Zero, null, 0);

            if (camera.HasFlags(RenderCameraFlags.ShowOverlay))
                renderer.Overlay.Render(time, renderer.SpriteBatcher, renderer.Profiler, context.Scene.Profiler, camera);

            renderer.SpriteBatcher.Flush(device, _orthoCamera, _dummyData);
            renderer.Device.EndDraw();
        }
    }
}

namespace Molten.Graphics
{
    internal class FinalizeStep : RenderStepBase
    {
        RenderCamera _orthoCamera;
        ObjectRenderData _dummyData;

        internal override void Initialize(RendererDX11 renderer)
        {
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

            // We only need scissor testing here
            StateConditions conditions = StateConditions.ScissorTest;
            ITexture2D sourceSurface = context.HasComposed ? context.PreviousComposition : renderer.Surfaces[MainSurfaceType.Scene];

            renderer.Device.BeginDraw(conditions);
            renderer.SpriteBatcher.Draw(sourceSurface, bounds, Vector2F.Zero, camera.OutputSurface.Viewport.Bounds.Size, Color.White, 0, Vector2F.Zero, null, 0);

            if (camera.HasFlags(RenderCameraFlags.ShowOverlay))
                renderer.Overlay.Render(time, renderer.SpriteBatcher, renderer.Profiler, context.Scene.Profiler, camera);

            renderer.SpriteBatcher.Flush(device, _orthoCamera, _dummyData);
            renderer.Device.EndDraw();
        }
    }
}

namespace Molten.Graphics
{
    internal class FinalizeStep : RenderStepBase
    {
        RenderCamera _orthoCamera;
        ObjectRenderData _dummyData;

        public override void Initialize(RenderService renderer)
        {
            _dummyData = new ObjectRenderData();
            _orthoCamera = new RenderCamera(RenderCameraMode.Orthographic);
        }

        public override void Dispose()
        {

        }

        public override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            _orthoCamera.Surface = camera.Surface;

            RectangleF bounds = new RectangleF(0, 0, camera.Surface.Width, camera.Surface.Height);
            CommandQueueDX11 cmd = renderer.Device.Cmd as CommandQueueDX11;
            IRenderSurface2D finalSurface = camera.Surface as RenderSurface2D;

            if (!camera.HasFlags(RenderCameraFlags.DoNotClear))
                renderer.ClearIfFirstUse(finalSurface, context.Scene.BackgroundColor);

            cmd.SetRenderSurfaces(null);
            cmd.SetRenderSurface(finalSurface, 0);
            cmd.DepthSurface.Value = null;
            cmd.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            cmd.SetViewports(camera.Surface.Viewport);
            cmd.SetScissorRectangle((Rectangle)camera.Surface.Viewport.Bounds);

            // We only need scissor testing here
            StateConditions conditions = StateConditions.ScissorTest;
            ITexture2D sourceSurface = context.HasComposed ? context.PreviousComposition : renderer.Surfaces[MainSurfaceType.Scene];

            RectStyle style = RectStyle.Default;

            cmd.BeginDraw(conditions);
            renderer.SpriteBatch.Draw(sourceSurface, bounds, Vector2F.Zero, camera.Surface.Viewport.Bounds.Size, 0, Vector2F.Zero, ref style, null, 0, 0);

            if (camera.HasFlags(RenderCameraFlags.ShowOverlay))
                renderer.Overlay.Render(time, renderer.SpriteBatch, renderer.Profiler, context.Scene.Profiler, camera);

            renderer.SpriteBatch.Flush(cmd, _orthoCamera, _dummyData);
            cmd.EndDraw();
        }
    }
}

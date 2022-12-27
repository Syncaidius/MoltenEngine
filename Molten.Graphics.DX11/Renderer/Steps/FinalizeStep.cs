using Molten.Font;

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

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext cxt, Timing time)
        {
            _orthoCamera.Surface = camera.Surface;

            RectangleF bounds = new RectangleF(0, 0, camera.Surface.Width, camera.Surface.Height);
            RenderSurface2D finalSurface = camera.Surface as RenderSurface2D;

            if (!camera.HasFlags(RenderCameraFlags.DoNotClear))
                renderer.ClearIfFirstUse(cxt.Context, finalSurface, cxt.Scene.BackgroundColor);

            cxt.Context.State.SetRenderSurfaces(null);
            cxt.Context.State.SetRenderSurface(finalSurface, 0);
            cxt.Context.State.DepthSurface.Value = null;
            cxt.Context.State.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            cxt.Context.State.SetViewports(camera.Surface.Viewport);
            cxt.Context.State.SetScissorRectangle((Rectangle)camera.Surface.Viewport.Bounds);

            // We only need scissor testing here
            StateConditions conditions = StateConditions.ScissorTest;
            ITexture2D sourceSurface = cxt.HasComposed ? cxt.PreviousComposition : renderer.Surfaces[MainSurfaceType.Scene];

            RectStyle style = RectStyle.Default;

            cxt.Context.BeginDraw(conditions);
            renderer.SpriteBatcher.Draw(sourceSurface, bounds, Vector2F.Zero, camera.Surface.Viewport.Bounds.Size, 0, Vector2F.Zero, ref style, null, 0, 0);

            if (camera.HasFlags(RenderCameraFlags.ShowOverlay))
                renderer.Overlay.Render(time, renderer.SpriteBatcher, renderer.Profiler, cxt.Scene.Profiler, camera);

            renderer.SpriteBatcher.Flush(cxt.Context, _orthoCamera, _dummyData);
            cxt.Context.EndDraw();
        }
    }
}

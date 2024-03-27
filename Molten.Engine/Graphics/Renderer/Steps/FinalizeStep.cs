namespace Molten.Graphics;

internal class FinalizeStep : RenderStep
{
    RenderCamera _orthoCamera;
    ObjectRenderData _dummyData;

    protected override void OnInitialize(RenderService renderer)
    {
        _dummyData = new ObjectRenderData();
        _orthoCamera = new RenderCamera(RenderCameraMode.Orthographic);
    }

    public override void Dispose()
    {

    }

    internal override void Draw(GpuCommandList cmd, RenderCamera camera, RenderChainContext context, Timing time)
    {
        _orthoCamera.Surface = camera.Surface;

        RectangleF bounds = new RectangleF(0, 0, camera.Surface.Width, camera.Surface.Height);
        
        if (!camera.HasFlags(RenderCameraFlags.DoNotClear))
            Renderer.Surfaces.ClearIfFirstUse(cmd, camera.Surface, camera.BackgroundColor);

        cmd.State.Surfaces.Reset();
        cmd.State.Surfaces[0] = camera.Surface;
        cmd.State.DepthSurface.Value = null;
        cmd.State.Viewports.Reset(camera.Surface.Viewport);
        cmd.State.ScissorRects.Reset((Rectangle)camera.Surface.Viewport.Bounds);

        // We only need scissor testing here
        IRenderSurface2D sourceSurface = context.HasComposed ? context.PreviousComposition : Renderer.Surfaces[MainSurfaceType.Scene];
        RectStyle style = RectStyle.Default;

        Renderer.SpriteBatch.Draw(sourceSurface, bounds, Vector2F.Zero, camera.Surface.Viewport.Size, 0, Vector2F.Zero, ref style, null, 0, 0);

        if (camera.HasFlags(RenderCameraFlags.ShowOverlay))
            Renderer.Overlay.Render(time, Renderer.SpriteBatch, Renderer.Profiler, camera);

        Renderer.SpriteBatch.Flush(cmd, _orthoCamera, _dummyData);

        cmd.ResetState();
    }
}

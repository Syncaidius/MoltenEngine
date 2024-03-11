namespace Molten.Graphics;

/// <summary>
/// The forward-rendering step.
/// </summary>
internal class ForwardStep : RenderStep
{
    public override void Dispose()
    { }

    protected override void OnInitialize(RenderService service) { }

    internal override void Draw(GpuCommandList cmd, RenderCamera camera, RenderChainContext context, Timing time)
    {
        if (context.Layer.Renderables.Count == 0)
            return;

        IRenderSurface2D sScene = Renderer.Surfaces[MainSurfaceType.Scene];

        cmd.State.Surfaces.Reset();
        cmd.State.Surfaces[0] = sScene;
        cmd.State.DepthSurface.Value = Renderer.Surfaces.GetDepth();

        cmd.State.Viewports.Reset(camera.Surface.Viewport);
        cmd.State.ScissorRects.Reset((Rectangle)camera.Surface.Viewport.Bounds);

        Renderer.RenderSceneLayer(cmd, context.Layer, camera);
    }
}

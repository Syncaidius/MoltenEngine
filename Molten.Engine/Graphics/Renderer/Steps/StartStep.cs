namespace Molten.Graphics;

internal class StartStep : RenderStep
{
    public override void Dispose() { }

    protected override void OnInitialize(RenderService service) { }

    internal override void Draw(GpuCommandList cmd, RenderCamera camera, RenderChainContext context, Timing time)
    {
        IRenderSurface2D sScene = Renderer.Surfaces[MainSurfaceType.Scene];
        IRenderSurface2D sNormals = Renderer.Surfaces[MainSurfaceType.Normals];
        IRenderSurface2D sEmissive = Renderer.Surfaces[MainSurfaceType.Emissive];
        IDepthStencilSurface sDepth = Renderer.Surfaces.GetDepth();

        cmd.State.Surfaces.Reset();
        sScene.ClearImmediate(cmd, camera.BackgroundColor);
        sNormals.ClearImmediate(cmd, Color.White * 0.5f);
        sEmissive.ClearImmediate(cmd, Color.Black);
        sDepth.ClearImmediate(cmd, DepthClearFlags.Depth | DepthClearFlags.Stencil, 1, 0);

        Renderer.SpriteBatch.Reset((Rectangle)camera.Surface.Viewport.Bounds);
    }
}

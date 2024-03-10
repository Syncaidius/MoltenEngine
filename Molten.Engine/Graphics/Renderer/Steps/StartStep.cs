namespace Molten.Graphics;

internal class StartStep : RenderStep
{
    public override void Dispose() { }

    protected override void OnInitialize(RenderService service) { }

    internal override void Render(GpuCommandQueue queue, RenderCamera camera, RenderChainContext context, Timing time)
    {
        IRenderSurface2D sScene = Renderer.Surfaces[MainSurfaceType.Scene];
        IRenderSurface2D sNormals = Renderer.Surfaces[MainSurfaceType.Normals];
        IRenderSurface2D sEmissive = Renderer.Surfaces[MainSurfaceType.Emissive];
        IDepthStencilSurface sDepth = Renderer.Surfaces.GetDepth();

        Renderer.Device.Queue.State.Surfaces.Reset();
        sScene.Clear(GpuPriority.Immediate, camera.BackgroundColor);
        sNormals.Clear(GpuPriority.Immediate, Color.White * 0.5f);
        sEmissive.Clear(GpuPriority.Immediate, Color.Black);
        sDepth.Clear(GpuPriority.Immediate, DepthClearFlags.Depth | DepthClearFlags.Stencil, 1, 0);

        Renderer.SpriteBatch.Reset((Rectangle)camera.Surface.Viewport.Bounds);
    }
}

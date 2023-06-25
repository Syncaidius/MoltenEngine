namespace Molten.Graphics
{
    /// <summary>
    /// The forward-rendering step.
    /// </summary>
    internal class ForwardStep : RenderStep
    {
        public override void Dispose()
        { }

        protected override void OnInitialize(RenderService service) { }

        internal override void Render(GraphicsQueue queue, RenderCamera camera, RenderChainContext context, Timing time)
        {
            if (context.Layer.Renderables.Count == 0)
                return;

            IRenderSurface2D sScene = Renderer.Surfaces[MainSurfaceType.Scene];

            queue.State.Surfaces.Reset();
            queue.State.Surfaces[0] = sScene;
            queue.State.DepthSurface.Value = Renderer.Surfaces.GetDepth();

            queue.State.Viewports.Reset(camera.Surface.Viewport);
            queue.State.ScissorRects.Reset((Rectangle)camera.Surface.Viewport.Bounds);

            Renderer.RenderSceneLayer(queue, context.Layer, camera);
        }
    }
}

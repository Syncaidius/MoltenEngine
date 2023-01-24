using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StartStep : RenderStepBase
    {
        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            IRenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            IRenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];
            IRenderSurface2D sEmissive = renderer.Surfaces[MainSurfaceType.Emissive];
            IDepthStencilSurface sDepth = renderer.Surfaces.GetDepth();

            CommandQueueDX11 cmd = renderer.NativeDevice.Cmd;

            cmd.SetRenderSurfaces(null);
            bool newSurface = renderer.ClearIfFirstUse(sScene, context.Scene.BackgroundColor);
            renderer.ClearIfFirstUse(sNormals, Color.White * 0.5f);
            renderer.ClearIfFirstUse(sEmissive, Color.Black);

            // Always clear the depth surface at the start of each scene unless otherwise instructed.
            // Will also be cleared if we've just switched to a previously un-rendered surface during this frame.
            if(!camera.Flags.HasFlag(RenderCameraFlags.DoNotClearDepth) || newSurface)
                sDepth.Clear(DepthClearFlags.Depth | DepthClearFlags.Stencil, GraphicsPriority.Immediate, 1, 0);
        }
    }
}

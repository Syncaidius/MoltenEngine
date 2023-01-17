using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StartStep : RenderStepBase
    {
        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            RenderSurface2D sScene = renderer.Surfaces[MainSurfaceType.Scene];
            RenderSurface2D sNormals = renderer.Surfaces[MainSurfaceType.Normals];
            RenderSurface2D sEmissive = renderer.Surfaces[MainSurfaceType.Emissive];
            DepthStencilSurface sDepth = renderer.Surfaces.GetDepth();

            CommandQueueDX11 cmd = renderer.NativeDevice.Cmd;

            cmd.State.SetRenderSurfaces(null);
            bool newSurface = renderer.ClearIfFirstUse(cmd, sScene, context.Scene.BackgroundColor);
            renderer.ClearIfFirstUse(cmd, sNormals, Color.White * 0.5f);
            renderer.ClearIfFirstUse(cmd, sEmissive, Color.Black);

            // Always clear the depth surface at the start of each scene unless otherwise instructed.
            // Will also be cleared if we've just switched to a previously un-rendered surface during this frame.
            if(!camera.Flags.HasFlag(RenderCameraFlags.DoNotClearDepth) || newSurface)
                sDepth.Clear(cmd, ClearFlag.Depth | ClearFlag.Stencil);
        }
    }
}

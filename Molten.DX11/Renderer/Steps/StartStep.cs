using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class StartStep : RenderStepBase
    {
        internal RenderSurface Scene;
        internal RenderSurface Normals;
        internal RenderSurface Emissive;
        internal DepthSurface Depth;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);
        }

        internal override void UpdateSurfaces(RendererDX11 renderer,int width, int height)
        {
            // Dispose of current surfaces
            DisposeSurfaces();

            Scene = new RenderSurface(renderer, width, height, Format.R8G8B8A8_UNorm);
            Normals = new RenderSurface(renderer, width, height, Format.R11G11B10_Float);
            Emissive = new RenderSurface(renderer, width, height, Format.R8G8B8A8_UNorm);
            Depth = new DepthSurface(renderer, width, height, DepthFormat.R24G8_Typeless);
        }

        private void DisposeSurfaces()
        {
            Scene?.Dispose();
            Normals?.Dispose();
            Emissive?.Dispose();
            Depth?.Dispose();
        }

        public override void Dispose()
        {
            DisposeSurfaces();
        }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData<Renderable> scene, Timing time, RenderChain.Link link)
        {
            GraphicsDeviceDX11 device = renderer.Device;

            scene.View = camera.View;
            if (camera.FinalSurface != camera.OutputSurface)
            {
                scene.Projection = Matrix4F.PerspectiveFovLH((float)Math.PI / 4.0f, camera.FinalSurface.Width / (float)camera.FinalSurface.Height, 0.1f, 1000.0f);
                scene.ViewProjection = scene.View * scene.Projection;
            }
            else
            {
                scene.Projection = camera.Projection;
                scene.ViewProjection = camera.ViewProjection;
            }

            // Clear the depth surface if it hasn't already been cleared
            scene.InvViewProjection = Matrix4F.Invert(scene.ViewProjection);

            bool newSurface = renderer.ClearIfFirstUse(device, Scene, scene.BackgroundColor);
            renderer.ClearIfFirstUse(device, Normals, Color.White * 0.5f);
            renderer.ClearIfFirstUse(device, Emissive, Color.Black);

            // Always clear the depth surface at the start of each scene unless otherwise instructed.
            // Will also be cleared if we've just switched to a previously un-rendered surface during this frame.
            if(!camera.Flags.HasFlag(RenderCameraFlags.DoNotClearDepth) || newSurface)
                Depth.Clear(device, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil);
        }
    }
}

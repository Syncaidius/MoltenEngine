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

            Scene = new RenderSurface(renderer.Device, width, height, Format.R8G8B8A8_UNorm);
            Normals = new RenderSurface(renderer.Device, width, height, Format.R11G11B10_Float);
            Emissive = new RenderSurface(renderer.Device, width, height, Format.R8G8B8A8_UNorm);
            Depth = new DepthSurface(renderer.Device, width, height, DepthFormat.R24G8_Typeless);
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

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            GraphicsDevice device = renderer.Device;

            if (scene.Camera != null)
            {
                scene.View = scene.Camera.View;
                if (scene.FinalSurface != scene.Camera.OutputSurface)
                {
                    scene.Projection = Matrix4F.PerspectiveFovLH((float)Math.PI / 4.0f, scene.FinalSurface.Width / (float)scene.FinalSurface.Height, 0.1f, 1000.0f);
                    scene.ViewProjection = scene.View * scene.Projection;
                }
                else
                {
                    scene.Projection = scene.Camera.Projection;
                    scene.ViewProjection = scene.Camera.ViewProjection;
                }
            }
            else
            {
                scene.View = RendererDX11.DefaultView3D;
                scene.Projection = Matrix4F.PerspectiveFovLH((float)Math.PI / 4.0f, scene.FinalSurface.Width / (float)scene.FinalSurface.Height, 0.1f, 1000.0f);
                scene.ViewProjection = scene.View * scene.Projection;
            }

            // Clear the depth surface if it hasn't already been cleared
            scene.InvViewProjection = Matrix4F.Invert(scene.ViewProjection);
            bool newSurface = renderer.ClearIfFirstUse(Scene, () => Scene.Clear(device, scene.BackgroundColor));
            renderer.ClearIfFirstUse(Normals, () => Normals.Clear(device, Color.White * 0.5f));
            renderer.ClearIfFirstUse(Emissive, () => Emissive.Clear(device, Color.Black));

            // Always clear the depth surface at the start of each scene unless otherwise instructed.
            // Will also be cleared if we've just switched to a previously un-rendered surface during this frame.
            if(!scene.HasFlag(SceneRenderFlags.DoNotClearDepth) || newSurface)
                Depth.Clear(device, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil);
        }
    }
}

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
        }

        private void DisposeSurfaces()
        {
            Scene?.Dispose();
            Normals?.Dispose();
            Emissive?.Dispose();
        }

        public override void Dispose()
        {
            DisposeSurfaces();
        }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            GraphicsDevice device = renderer.Device;

            if (scene.RenderCamera != null)
            {
                scene.Projection = scene.RenderCamera.Projection;
                scene.View = scene.RenderCamera.View;
                scene.ViewProjection = scene.RenderCamera.ViewProjection;
            }
            else
            {
                scene.View = RendererDX11.DefaultView3D;
                scene.Projection = Matrix4F.PerspectiveFovLH((float)Math.PI / 4.0f, scene.FinalSurface.Width / (float)scene.FinalSurface.Height, 0.1f, 100.0f);
                scene.ViewProjection = Matrix4F.Multiply(scene.View, scene.Projection);
            }

            // Clear the depth surface if it hasn't already been cleared
            renderer.ClearIfFirstUse(Scene, () => Scene.Clear(device, scene.BackgroundColor));
            renderer.ClearIfFirstUse(Normals, () => Normals.Clear(device, scene.BackgroundColor));
            renderer.ClearIfFirstUse(Emissive, () => Emissive.Clear(device, scene.BackgroundColor));

            if(scene.FinalDepthSurface != null)
                renderer.ClearIfFirstUse(scene.FinalDepthSurface, () => scene.FinalDepthSurface.Clear(device, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil));

            device.Rasterizer.SetViewports(scene.FinalSurface.Viewport);
            device.SetDepthSurface(scene.FinalDepthSurface, GraphicsDepthMode.Enabled);
            device.DepthStencil.SetPreset(DepthStencilPreset.Default);
        }
    }
}

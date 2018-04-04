using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class Render2dStep : RenderStepBase
    {
        static readonly Matrix4F _defaultView2D = Matrix4F.Identity;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);
        }

        internal override void UpdateSurfaces(RendererDX11 renderer,int width, int height)
        {

        }

        public override void Dispose()
        {

        }

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            Matrix4F spriteView, spriteProj, spriteViewProj;
            RenderSurfaceBase rs = scene.FinalSurface;
            DepthSurface ds = scene.FinalDepthSurface;

            GraphicsDevice device = renderer.Device;

            if (scene.Camera != null)
            {
                spriteProj = scene.Camera.Projection;
                spriteView = scene.Camera.View;
                spriteViewProj = scene.Camera.ViewProjection;
            }
            else
            {
                spriteProj = _defaultView2D;
                spriteView = Matrix4F.OrthoOffCenterLH(0, rs.Width, -rs.Height, 0, 0, 1);
                spriteViewProj = Matrix4F.Multiply(spriteView, spriteProj);
            }

            if (rs != null)
            {
                if (!scene.HasFlag(SceneRenderFlags.DoNotClear))
                    renderer.ClearIfFirstUse(rs, () => rs.Clear(scene.BackgroundColor));

                // Clear the depth surface if it hasn't already been cleared
                if (ds != null)
                    renderer.ClearIfFirstUse(ds, () => ds.Clear(DepthClearFlags.Depth | DepthClearFlags.Stencil));

                device.SetRenderSurfaces(null);
                device.SetRenderSurface(rs, 0);
                device.SetDepthSurface(ds, GraphicsDepthMode.Enabled);
                device.DepthStencil.SetPreset(DepthStencilPreset.Default);
                device.Rasterizer.SetViewports(rs.Viewport);

                renderer.SpriteBatcher.Begin(rs.Viewport);
                scene.Render2D(device, renderer);
                renderer.SpriteBatcher.Flush(device, ref spriteViewProj, rs.SampleCount > 1);
            }
        }
    }
}

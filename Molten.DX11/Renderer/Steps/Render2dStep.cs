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

        internal override void Render(RendererDX11 renderer, SceneRenderData<Renderable> scene, Timing time, RenderChain.Link link)
        {
            Matrix4F spriteView, spriteProj, spriteViewProj;
            RenderSurfaceBase rs = scene.FinalSurface as RenderSurfaceBase;

            GraphicsDeviceDX11 device = renderer.Device;

            if (scene.Camera != null && scene.Camera.OutputSurface != null)
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

                device.SetRenderSurfaces(null);
                device.SetRenderSurface(rs, 0);
                device.SetDepthSurface(null, GraphicsDepthMode.Disabled);
                device.Rasterizer.SetViewports(rs.Viewport);

                StateConditions conditions = StateConditions.ScissorTest;
                conditions |= rs.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

                device.BeginDraw(conditions);
                renderer.SpriteBatcher.Begin(rs.Viewport);
                renderer.Render2D(device, scene);
                renderer.SpriteBatcher.End(device, ref spriteViewProj, rs);
                device.EndDraw();
            }
        }
    }
}

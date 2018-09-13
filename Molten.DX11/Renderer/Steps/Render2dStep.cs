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

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData sceneData, LayerRenderData<Renderable> layerData, Timing time, RenderChain.Link link)
        {
            GraphicsDeviceDX11 device = renderer.Device;
            Matrix4F spriteView, spriteProj, spriteViewProj;
            RenderSurfaceBase rs = camera.OutputSurface as RenderSurfaceBase;

            spriteProj = camera.Projection;
            spriteView = camera.View;
            spriteViewProj = camera.ViewProjection;

            if (!camera.Flags.HasFlag(RenderCameraFlags.DoNotClear))
                renderer.ClearIfFirstUse(device, rs, sceneData.BackgroundColor);

            device.SetRenderSurfaces(null);
            device.SetRenderSurface(rs, 0);
            device.SetDepthSurface(null, GraphicsDepthMode.Disabled);
            device.Rasterizer.SetViewports(rs.Viewport);

            StateConditions conditions = StateConditions.ScissorTest;
            conditions |= rs.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

            device.BeginDraw(conditions);
            renderer.SpriteBatcher.Begin(rs.Viewport);

            // Draw 2D objects.
            for (int j = 0; j < layerData.Renderables2D.Count; j++)
                layerData.Renderables2D[j].Render(renderer.SpriteBatcher);

            renderer.SpriteBatcher.End(device, ref spriteViewProj, rs);
            device.EndDraw();
        }
    }
}

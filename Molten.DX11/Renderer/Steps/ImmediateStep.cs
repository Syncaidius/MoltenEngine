using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Renders the 3D scene directly to it's output.
    /// </summary>
    internal class Immediate3dStep : RenderStepBase
    {
        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);
        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height)
        { }

        public override void Dispose()
        { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData sceneData, LayerRenderData<Renderable> layerData, Timing time, RenderChain.Link link)
        {
            RenderSurfaceBase rs = null;
            GraphicsDeviceDX11 device = renderer.Device;

            rs = camera.OutputSurface as RenderSurfaceBase;
            switch (link.Previous.Step)
            {
                case StartStep start:

                    if (!camera.Flags.HasFlag(RenderCameraFlags.DoNotClear))
                        renderer.ClearIfFirstUse(device, rs, sceneData.BackgroundColor);

                    device.SetRenderSurface(rs, 0);
                    device.SetDepthSurface(start.Depth, GraphicsDepthMode.Enabled);
                    device.Rasterizer.SetViewports(rs.Viewport);
                    device.Rasterizer.SetScissorRectangle(rs.Viewport.Bounds);

                    StateConditions conditions = StateConditions.ScissorTest; // TODO expand
                    conditions |= rs.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

                    device.BeginDraw(conditions);
                    renderer.RenderSceneLayer(device, layerData, camera);
                    device.EndDraw();
                    break;
            }
        }
    }
}

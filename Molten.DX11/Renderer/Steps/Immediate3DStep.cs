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

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData<Renderable> scene, Timing time, RenderChain.Link link)
        {
            RenderSurfaceBase rs = null;
            GraphicsDeviceDX11 device = renderer.Device;

            rs = camera.OutputSurface as RenderSurfaceBase;
            scene.Projection = camera.Projection;
            scene.View = camera.View;
            scene.ViewProjection = camera.ViewProjection;

            switch (link.Previous.Step)
            {
                case StartStep start:

                    if (!camera.Flags.HasFlag(RenderCameraFlags.DoNotClear))
                        renderer.ClearIfFirstUse(device, rs, scene.BackgroundColor);

                    device.SetRenderSurface(rs, 0);
                    device.SetDepthSurface(start.Depth, GraphicsDepthMode.Enabled);
                    device.Rasterizer.SetViewports(rs.Viewport);

                    StateConditions conditions = StateConditions.None; // TODO expand
                    device.BeginDraw(conditions);
                    renderer.Render3D(device, scene);
                    device.EndDraw();
                    break;
            }
        }
    }
}

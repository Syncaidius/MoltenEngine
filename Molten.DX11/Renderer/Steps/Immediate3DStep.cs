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

        internal override void Render(RendererDX11 renderer, SceneRenderData<Renderable> scene, Timing time, RenderChain.Link link)
        {
            RenderSurfaceBase rs = null;
            GraphicsDeviceDX11 device = renderer.Device;

            if (scene.Camera != null)
            {
                rs = scene.Camera.OutputSurface as RenderSurfaceBase;
                rs = rs ?? device.DefaultSurface;

                scene.Projection = scene.Camera.Projection;
                scene.View = scene.Camera.View;
                scene.ViewProjection = scene.Camera.ViewProjection;
            }
            else
            {
                rs = device.DefaultSurface;
                if (rs == null)
                    return;

                scene.View = RendererDX11.DefaultView3D;
                scene.Projection = Matrix4F.PerspectiveFovLH((float)Math.PI / 4.0f, rs.Width / (float)rs.Height, 0.1f, 100.0f);
                scene.ViewProjection = Matrix4F.Multiply(scene.View, scene.Projection);
            }

            if (rs != null)
            {
                switch (link.Previous.Step)
                {
                    case StartStep start:

                        if (!scene.HasFlag(SceneRenderFlags.DoNotClear))
                            renderer.ClearIfFirstUse(rs, () => rs.Clear(scene.BackgroundColor));

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
}

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
    internal class Immediate3dStep : DeferredRenderStep
    {
        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);
        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height)
        { }

        public override void Dispose()
        { }

        protected override void OnRender(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time)
        {
            RenderSurfaceBase rs = null;
            DepthSurface ds = null;
            GraphicsDevice device = renderer.Device;

            if (scene.RenderCamera != null)
            {
                rs = scene.RenderCamera.OutputSurface as RenderSurfaceBase;
                ds = scene.RenderCamera.OutputDepthSurface as DepthSurface;
                rs = rs ?? device.DefaultSurface;

                scene.Projection = scene.RenderCamera.Projection;
                scene.View = scene.RenderCamera.View;
                scene.ViewProjection = scene.RenderCamera.ViewProjection;
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
                if (!scene.HasFlag(SceneRenderFlags.DoNotClear))
                    renderer.ClearIfFirstUse(rs, () => rs.Clear(scene.BackgroundColor));

                // Clear the depth surface if it hasn't already been cleared
                if (ds != null)
                    renderer.ClearIfFirstUse(ds, () => ds.Clear(device, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil));

                device.SetRenderSurface(rs, 0);
                device.SetDepthSurface(ds, GraphicsDepthMode.Enabled);
                device.DepthStencil.SetPreset(DepthStencilPreset.Default);
                device.Rasterizer.SetViewports(rs.Viewport);
                scene.Render3D(device, renderer);
            }
        }
    }
}

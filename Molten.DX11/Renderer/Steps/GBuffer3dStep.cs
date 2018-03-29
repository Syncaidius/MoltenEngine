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
    internal class GBuffer3dStep : DeferredRenderStep
    {
        internal RenderSurface Scene;
        internal RenderSurface Normals;
        internal RenderSurface Emissive;

        Material _matStandard;
        Material _matSansNormalMap;
        IShaderValue _texDiffuse;
        IShaderValue _texNormal;
        IShaderValue _texEmissive;

        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);

            string source = null;
            string namepace = "Molten.Graphics.Assets.gbuffer.sbm";
            using (Stream stream = EmbeddedResource.GetStream(namepace, typeof(RendererDX11).Assembly))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                ShaderCompileResult result = renderer.ShaderCompiler.Compile(source, namepace);
                _matStandard = result["material", "gbuffer"] as Material;
                _matSansNormalMap = result["material", "gbuffer-sans-nmap"] as Material;
            }
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

        protected override void OnRender(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time)
        {
            GraphicsDevice device = renderer.Device;
            DepthSurface ds = null;
            RenderSurfaceBase rs = null;

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

            // Clear surfaces
            Scene.Clear(renderer.Device, scene.BackgroundColor);
            Normals.Clear(renderer.Device, Color.White * 0.5f);
            Emissive.Clear(renderer.Device, Color.Black);

            // Clear the depth surface if it hasn't already been cleared
            renderer.ClearIfFirstUse(Scene, () => Scene.Clear(device, scene.BackgroundColor));
            renderer.ClearIfFirstUse(Normals, () => Normals.Clear(device, scene.BackgroundColor));
            renderer.ClearIfFirstUse(Emissive, () => Emissive.Clear(device, scene.BackgroundColor));
            renderer.ClearIfFirstUse(ds, () => ds.Clear(device, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil));

            device.SetRenderSurface(Scene, 0);
            device.SetRenderSurface(Normals, 1);
            device.SetRenderSurface(Emissive, 2);
            device.SetDepthSurface(ds, GraphicsDepthMode.Enabled);
            device.DepthStencil.SetPreset(DepthStencilPreset.Default);
            device.Rasterizer.SetViewports(rs.Viewport);
            scene.Render3D(device, renderer);
        }
    }
}

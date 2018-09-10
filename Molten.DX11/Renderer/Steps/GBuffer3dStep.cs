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
    internal class GBuffer3dStep : RenderStepBase
    {
        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {

        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height) { }

        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData<Renderable> scene, Timing time, RenderChain.Link link)
        {
            GraphicsDeviceDX11 device = renderer.Device;

            switch (link.Previous.Step)
            {
                case StartStep start:
                    device.SetRenderSurface(start.Scene, 0);
                    device.SetRenderSurface(start.Normals, 1);
                    device.SetRenderSurface(start.Emissive, 2);
                    device.SetDepthSurface(start.Depth, GraphicsDepthMode.Enabled);

                    SetMaterialCommon(renderer.StandardMeshMaterial, camera, scene, start.Scene);
                    SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, camera, scene, start.Scene);
                    break;

                    // TODO add alternate HDR start step here (which should be used in conjunction HDR textures, HDR RTs and so on).
            }

            device.Rasterizer.SetViewports(camera.OutputSurface.Viewport);
            StateConditions conditions = StateConditions.None; // TODO expand
            device.BeginDraw(conditions);
            renderer.Render3D(device, scene);
            device.EndDraw();
        }

        private void SetMaterialCommon(Material material, RenderCamera camera, SceneRenderData scene, RenderSurface gBufferScene)
        {
            material.Scene.View.Value = scene.View;
            material.Scene.Projection.Value = scene.Projection;
            material.Scene.InvViewProjection.Value = Matrix4F.Invert(scene.ViewProjection);
            material.Scene.ViewProjection.Value = scene.ViewProjection;
            material.Scene.MaxSurfaceUV.Value = new Vector2F()
            {
                X = (float)camera.OutputSurface.Width / gBufferScene.Width,
                Y = (float)camera.OutputSurface.Height / gBufferScene.Height,
            };
        }
    }
}

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
    internal class GBufferStep : RenderStepBase
    {
        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {

        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height) { }

        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData sceneData, LayerRenderData<Renderable> layerData, Timing time, RenderChain.Link link)
        {
            GraphicsDeviceDX11 device = renderer.Device;

            switch (link.Previous.Step)
            {
                case StartStep start:
                    device.SetRenderSurface(start.Scene, 0);
                    device.SetRenderSurface(start.Normals, 1);
                    device.SetRenderSurface(start.Emissive, 2);
                    device.SetDepthSurface(start.Depth, GraphicsDepthMode.Enabled);

                    SetMaterialCommon(renderer.StandardMeshMaterial, camera, start.Scene);
                    SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, camera, start.Scene);
                    break;

                    // TODO add alternate HDR start step here (which should be used in conjunction HDR textures, HDR RTs and so on).
            }

            device.Rasterizer.SetViewports(camera.OutputSurface.Viewport);
            StateConditions conditions = StateConditions.None; // TODO expand
            conditions |= camera.OutputSurface.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

            device.BeginDraw(conditions);
            renderer.RenderSceneLayer(device, layerData, camera);
            device.EndDraw();
        }

        private void SetMaterialCommon(Material material, RenderCamera camera, RenderSurface gBufferScene)
        {
            material.Scene.View.Value = camera.View;
            material.Scene.Projection.Value = camera.Projection;
            material.Scene.InvViewProjection.Value = Matrix4F.Invert(camera.ViewProjection);
            material.Scene.ViewProjection.Value = camera.ViewProjection;
            material.Scene.MaxSurfaceUV.Value = new Vector2F()
            {
                X = (float)camera.OutputSurface.Width / gBufferScene.Width,
                Y = (float)camera.OutputSurface.Height / gBufferScene.Height,
            };
        }
    }
}

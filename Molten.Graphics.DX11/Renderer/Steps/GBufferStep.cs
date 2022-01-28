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
        RenderSurface _surfaceScene;
        RenderSurface _surfaceNormals;
        RenderSurface _surfaceEmissive;
        DepthStencilSurface _surfaceDepth;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.GetSurface<RenderSurface>(MainSurfaceType.Scene);
            _surfaceNormals = renderer.GetSurface<RenderSurface>(MainSurfaceType.Normals);
            _surfaceEmissive = renderer.GetSurface<RenderSurface>(MainSurfaceType.Emissive);
            _surfaceDepth = renderer.GetDepthSurface();
        }

        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            Device device = renderer.Device;

            device.SetRenderSurface(_surfaceScene, 0);
            device.SetRenderSurface(_surfaceNormals, 1);
            device.SetRenderSurface(_surfaceEmissive, 2);
            device.Output.DepthSurface.Value = _surfaceDepth;

            SetMaterialCommon(renderer.StandardMeshMaterial, camera, _surfaceScene);
            SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, camera, _surfaceScene);

            device.Rasterizer.SetViewports(camera.OutputSurface.Viewport);
            StateConditions conditions = StateConditions.None; // TODO expand
            conditions |= camera.OutputSurface.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

            device.BeginDraw(conditions);
            renderer.RenderSceneLayer(device, context.Layer, camera);
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

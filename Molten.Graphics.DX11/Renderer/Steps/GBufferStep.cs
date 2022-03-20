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
        RenderSurface2D _surfaceScene;
        RenderSurface2D _surfaceNormals;
        RenderSurface2D _surfaceEmissive;
        DepthStencilSurface _surfaceDepth;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Scene);
            _surfaceNormals = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Normals);
            _surfaceEmissive = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Emissive);
            _surfaceDepth = renderer.GetDepthSurface();
        }

        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            Device device = renderer.Device;

            device.State.SetRenderSurface(_surfaceScene, 0);
            device.State.SetRenderSurface(_surfaceNormals, 1);
            device.State.SetRenderSurface(_surfaceEmissive, 2);
            device.State.DepthSurface.Value = _surfaceDepth;

            SetMaterialCommon(renderer.StandardMeshMaterial, camera, _surfaceScene);
            SetMaterialCommon(renderer.StandardMeshMaterial_NoNormalMap, camera, _surfaceScene);

            device.State.SetViewports(camera.OutputSurface.Viewport);
            StateConditions conditions = StateConditions.None; // TODO expand
            conditions |= camera.OutputSurface.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

            device.BeginDraw(conditions);
            renderer.RenderSceneLayer(device, context.Layer, camera);
            device.EndDraw();
        }

        private void SetMaterialCommon(Material material, RenderCamera camera, RenderSurface2D gBufferScene)
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
